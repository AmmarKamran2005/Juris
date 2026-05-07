using Juris.Domain.Enums;

namespace Juris.Application.Analytics;

public sealed record AnalyticsEventInput(
    AnalyticsEventType Type,
    Guid? FirmId = null,
    Guid? SchoolId = null,
    Guid? ArticleId = null,
    string? UserId = null,
    Guid? SessionId = null,
    string? UniversityAffiliation = null,
    YearOfStudy YearOfStudy = YearOfStudy.Unknown,
    string? SearchTerm = null,
    string? SectionKey = null,
    int? DwellSeconds = null,
    string Path = "",
    string? Referrer = null,
    string? UserAgent = null,
    string? IpHash = null,
    string? MetadataJson = null
);

public sealed record TimeSeriesPoint(DateTime Date, int Count);

public sealed record UniversityBreakdown(string University, int Count, double Percent);
public sealed record YearOfStudyBreakdown(YearOfStudy YearOfStudy, int Count, double Percent);
public sealed record PerkClickBreakdown(string PerkCategory, string PerkName, int Clicks);
public sealed record SearchTermBreakdown(string Term, int Count);

/// <summary>
/// The HR Dashboard payload — every section maps to a chart in the Verified portal.
/// </summary>
public sealed record FirmAnalyticsDashboardDto(
    Guid FirmId,
    string FirmName,
    DateTime FromUtc,
    DateTime ToUtc,
    int ProfileViews,
    int UniqueSessions,
    int ApplyClicks,
    int EarlyCareersClicks,
    double ApplyClickThroughRate,
    int AverageDwellSeconds,
    IReadOnlyList<TimeSeriesPoint> ProfileViewsByDay,
    IReadOnlyList<UniversityBreakdown> TopUniversities,
    IReadOnlyList<YearOfStudyBreakdown> ByYearOfStudy,
    IReadOnlyList<PerkClickBreakdown> TopPerkClicks,
    IReadOnlyList<SectionDwellBreakdown> SectionDwell,
    IReadOnlyList<SearchTermBreakdown> RelevantSearchTerms,
    PeerBenchmarkDto Peer,
    DropOffSnapshotDto DropOff
);

public sealed record SectionDwellBreakdown(string SectionKey, int AvgDwellSeconds, int Samples);
public sealed record PeerBenchmarkDto(string PeerGroup, int FirmViews, int PeerAverageViews, double DiffPercent);
public sealed record DropOffSnapshotDto(int ExitsAfterBillableTargetView, int ExitsAfterRetentionView, int Total);

public sealed record GlobalAnalyticsSnapshotDto(
    int TotalEventsToday,
    int FirmProfileViewsToday,
    int SchoolProfileViewsToday,
    int ArticleViewsToday,
    int NewsletterSignupsToday,
    int ApplyClicksToday,
    IReadOnlyList<SearchTermBreakdown> TopSearchTerms,
    IReadOnlyList<TimeSeriesPoint> EventsByDayLast30
);
