using Juris.Application.Common;
using Juris.Domain.Entities;
using Juris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _db;

    public AnalyticsService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task TrackAsync(AnalyticsEventInput input, CancellationToken ct = default)
    {
        // Drop unknown FK references rather than failing the whole event.
        var firmId = input.FirmId.HasValue && await _db.Firms.AnyAsync(f => f.Id == input.FirmId, ct) ? input.FirmId : null;
        var schoolId = input.SchoolId.HasValue && await _db.Schools.AnyAsync(s => s.Id == input.SchoolId, ct) ? input.SchoolId : null;
        var articleId = input.ArticleId.HasValue && await _db.Articles.AnyAsync(a => a.Id == input.ArticleId, ct) ? input.ArticleId : null;

        var ev = new AnalyticsEvent
        {
            Type = input.Type,
            TimestampUtc = DateTime.UtcNow,
            FirmId = firmId,
            SchoolId = schoolId,
            ArticleId = articleId,
            UserId = input.UserId,
            SessionId = input.SessionId ?? Guid.NewGuid(),
            UniversityAffiliation = input.UniversityAffiliation,
            YearOfStudy = input.YearOfStudy,
            SearchTerm = input.SearchTerm,
            SectionKey = input.SectionKey,
            DwellSeconds = input.DwellSeconds,
            Path = string.IsNullOrEmpty(input.Path) ? "/" : input.Path,
            Referrer = input.Referrer,
            UserAgent = input.UserAgent,
            IpHash = input.IpHash,
            MetadataJson = input.MetadataJson,
        };
        _db.AnalyticsEvents.Add(ev);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<FirmAnalyticsDashboardDto?> GetFirmDashboardAsync(Guid firmId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var firm = await _db.Firms.AsNoTracking().FirstOrDefaultAsync(f => f.Id == firmId, ct);
        if (firm is null) return null;

        var events = await _db.AnalyticsEvents.AsNoTracking()
            .Where(e => e.FirmId == firmId && e.TimestampUtc >= fromUtc && e.TimestampUtc < toUtc)
            .ToListAsync(ct);

        var profileViews = events.Count(e => e.Type == AnalyticsEventType.FirmProfileView);
        var uniqueSessions = events.Where(e => e.Type == AnalyticsEventType.FirmProfileView).Select(e => e.SessionId).Distinct().Count();
        var applyClicks = events.Count(e => e.Type == AnalyticsEventType.ApplyButtonClick);
        var earlyClicks = events.Count(e => e.Type == AnalyticsEventType.EarlyCareersClick);
        var ctr = profileViews > 0 ? (applyClicks + earlyClicks) / (double)profileViews : 0;

        var dwellSamples = events.Where(e => e.Type == AnalyticsEventType.SectionDwell && e.DwellSeconds.HasValue).ToList();
        var avgDwell = dwellSamples.Count > 0 ? (int)dwellSamples.Average(e => e.DwellSeconds!.Value) : 0;

        var byDay = events
            .Where(e => e.Type == AnalyticsEventType.FirmProfileView)
            .GroupBy(e => e.TimestampUtc.Date)
            .Select(g => new TimeSeriesPoint(g.Key, g.Count()))
            .OrderBy(p => p.Date)
            .ToList();

        // Fill empty days for clean charts
        var filled = new List<TimeSeriesPoint>();
        for (var d = fromUtc.Date; d < toUtc.Date; d = d.AddDays(1))
        {
            var existing = byDay.FirstOrDefault(p => p.Date == d);
            filled.Add(existing ?? new TimeSeriesPoint(d, 0));
        }

        var totalUniHits = events.Count(e => !string.IsNullOrEmpty(e.UniversityAffiliation));
        var topUnis = events
            .Where(e => !string.IsNullOrEmpty(e.UniversityAffiliation))
            .GroupBy(e => e.UniversityAffiliation!)
            .Select(g => new UniversityBreakdown(g.Key, g.Count(),
                totalUniHits == 0 ? 0 : Math.Round(g.Count() * 100.0 / totalUniHits, 1)))
            .OrderByDescending(b => b.Count)
            .Take(8)
            .ToList();

        var totalYearHits = events.Count(e => e.YearOfStudy != YearOfStudy.Unknown);
        var byYear = events
            .Where(e => e.YearOfStudy != YearOfStudy.Unknown)
            .GroupBy(e => e.YearOfStudy)
            .Select(g => new YearOfStudyBreakdown(g.Key, g.Count(),
                totalYearHits == 0 ? 0 : Math.Round(g.Count() * 100.0 / totalYearHits, 1)))
            .OrderByDescending(b => b.Count)
            .ToList();

        var perkClicks = events
            .Where(e => e.Type == AnalyticsEventType.PerkClick && !string.IsNullOrEmpty(e.MetadataJson))
            .GroupBy(e => e.MetadataJson!)
            .Select(g =>
            {
                // Lazy parse: we stored "category|name"
                var parts = g.Key.Split('|', 2);
                return new PerkClickBreakdown(
                    parts.Length > 0 ? parts[0] : "Unknown",
                    parts.Length > 1 ? parts[1] : "Unknown",
                    g.Count());
            })
            .OrderByDescending(p => p.Clicks)
            .Take(10)
            .ToList();

        var sectionDwell = events
            .Where(e => e.Type == AnalyticsEventType.SectionDwell && e.DwellSeconds.HasValue && !string.IsNullOrEmpty(e.SectionKey))
            .GroupBy(e => e.SectionKey!)
            .Select(g => new SectionDwellBreakdown(g.Key,
                (int)g.Average(e => e.DwellSeconds!.Value),
                g.Count()))
            .OrderByDescending(s => s.AvgDwellSeconds)
            .ToList();

        var searchTerms = await _db.AnalyticsEvents.AsNoTracking()
            .Where(e => e.TimestampUtc >= fromUtc && e.TimestampUtc < toUtc &&
                        e.Type == AnalyticsEventType.SearchQuery && !string.IsNullOrEmpty(e.SearchTerm))
            .GroupBy(e => e.SearchTerm!)
            .Select(g => new { Term = g.Key, Count = g.Count() })
            .OrderByDescending(t => t.Count)
            .Take(10)
            .ToListAsync(ct);

        var peerAverage = await _db.AnalyticsEvents.AsNoTracking()
            .Where(e => e.TimestampUtc >= fromUtc && e.TimestampUtc < toUtc &&
                        e.Type == AnalyticsEventType.FirmProfileView &&
                        e.Firm != null && e.Firm.PeerGroup == firm.PeerGroup && e.FirmId != firmId)
            .GroupBy(e => e.FirmId)
            .Select(g => g.Count())
            .ToListAsync(ct);
        var peerAvg = peerAverage.Count > 0 ? (int)peerAverage.Average() : 0;
        var peerDiff = peerAvg > 0 ? Math.Round((profileViews - peerAvg) * 100.0 / peerAvg, 1) : 0;

        var dropOff = new DropOffSnapshotDto(
            events.Count(e => e.Type == AnalyticsEventType.DropOffSignal && e.SectionKey == "billable-target"),
            events.Count(e => e.Type == AnalyticsEventType.DropOffSignal && e.SectionKey == "retention-rate"),
            events.Count(e => e.Type == AnalyticsEventType.DropOffSignal));

        return new FirmAnalyticsDashboardDto(
            firmId, firm.Name, fromUtc, toUtc,
            profileViews, uniqueSessions, applyClicks, earlyClicks, Math.Round(ctr, 4),
            avgDwell, filled, topUnis, byYear, perkClicks, sectionDwell,
            searchTerms.Select(t => new SearchTermBreakdown(t.Term, t.Count)).ToList(),
            new PeerBenchmarkDto(firm.PeerGroup.ToString(), profileViews, peerAvg, peerDiff),
            dropOff);
    }

    public async Task<GlobalAnalyticsSnapshotDto> GetGlobalSnapshotAsync(CancellationToken ct = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var thirtyAgo = todayUtc.AddDays(-29);

        var todayEvents = await _db.AnalyticsEvents.AsNoTracking()
            .Where(e => e.TimestampUtc >= todayUtc).ToListAsync(ct);

        // GroupBy on .Date isn't always translated by EF Core; materialize first.
        var timestamps = await _db.AnalyticsEvents.AsNoTracking()
            .Where(e => e.TimestampUtc >= thirtyAgo)
            .Select(e => e.TimestampUtc)
            .ToListAsync(ct);
        var grouped = timestamps
            .GroupBy(t => t.Date)
            .Select(g => new TimeSeriesPoint(g.Key, g.Count()))
            .OrderBy(p => p.Date)
            .ToList();
        // Fill missing days for clean sparkline rendering.
        var byDay = new List<TimeSeriesPoint>();
        for (var d = thirtyAgo; d <= todayUtc; d = d.AddDays(1))
        {
            var existing = grouped.FirstOrDefault(p => p.Date == d);
            byDay.Add(existing ?? new TimeSeriesPoint(d, 0));
        }

        var topSearchesRaw = await _db.AnalyticsEvents.AsNoTracking()
            .Where(e => e.Type == AnalyticsEventType.SearchQuery && !string.IsNullOrEmpty(e.SearchTerm) && e.TimestampUtc >= thirtyAgo)
            .GroupBy(e => e.SearchTerm!)
            .Select(g => new { Term = g.Key, Count = g.Count() })
            .OrderByDescending(t => t.Count)
            .Take(10)
            .ToListAsync(ct);
        var topSearches = topSearchesRaw.Select(t => new SearchTermBreakdown(t.Term, t.Count)).ToList();

        return new GlobalAnalyticsSnapshotDto(
            TotalEventsToday: todayEvents.Count,
            FirmProfileViewsToday: todayEvents.Count(e => e.Type == AnalyticsEventType.FirmProfileView),
            SchoolProfileViewsToday: todayEvents.Count(e => e.Type == AnalyticsEventType.SchoolProfileView),
            ArticleViewsToday: todayEvents.Count(e => e.Type == AnalyticsEventType.ArticleView),
            NewsletterSignupsToday: todayEvents.Count(e => e.Type == AnalyticsEventType.NewsletterSignup),
            ApplyClicksToday: todayEvents.Count(e => e.Type == AnalyticsEventType.ApplyButtonClick),
            TopSearchTerms: topSearches,
            EventsByDayLast30: byDay);
    }
}
