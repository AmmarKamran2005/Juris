using Juris.Application.Common;
using Juris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Recruitment;

public class RecruitmentService : IRecruitmentService
{
    private readonly IApplicationDbContext _db;

    public RecruitmentService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RecruitmentDeadlineDto>> GetUpcomingAsync(int take, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var rows = await _db.FirmDeadlines.AsNoTracking()
            .Include(d => d.Firm)
            .Where(d => d.DeadlineUtc >= now && d.Firm!.IsPublished)
            .OrderBy(d => d.DeadlineUtc)
            .Take(take)
            .ToListAsync(ct);

        return rows.Select(d => Map(d, now)).ToList();
    }

    public async Task<IReadOnlyList<RecruitmentDeadlineDto>> ListAsync(DeadlineType? type, DeadlineStatus? status, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var query = _db.FirmDeadlines.AsNoTracking()
            .Include(d => d.Firm)
            .Where(d => d.Firm!.IsPublished);

        if (type.HasValue)
            query = query.Where(d => d.Type == type.Value);

        var rows = await query.OrderBy(d => d.DeadlineUtc).ToListAsync(ct);
        var mapped = rows.Select(d => Map(d, now));
        if (status.HasValue)
            mapped = mapped.Where(d => d.Status == status.Value);
        return mapped.ToList();
    }

    private static RecruitmentDeadlineDto Map(Domain.Entities.FirmDeadline d, DateTime nowUtc)
    {
        var status = d.ComputeStatus(nowUtc);
        var daysRemaining = (int)Math.Max(0, Math.Ceiling((d.DeadlineUtc - nowUtc).TotalDays));
        return new RecruitmentDeadlineDto(
            d.Firm!.Id, d.Firm!.Slug, d.Firm!.Name, d.Firm!.City, d.Firm!.LogoUrl,
            d.Type, d.Label, d.DeadlineUtc, d.ApplyUrl, status, daysRemaining,
            d.Firm!.IsVerified);
    }
}
