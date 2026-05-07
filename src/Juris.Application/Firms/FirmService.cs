using Juris.Application.Common;
using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Firms;

public class FirmService : IFirmService
{
    private readonly IApplicationDbContext _db;

    public FirmService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<FirmListItemDto>> SearchAsync(FirmFilter filter, CancellationToken ct = default)
    {
        var query = _db.Firms.AsNoTracking().Where(f => f.IsPublished);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(f =>
                EF.Functions.Like(f.Name, $"%{s}%") ||
                EF.Functions.Like(f.City, $"%{s}%") ||
                (f.VibeSummary != null && EF.Functions.Like(f.VibeSummary, $"%{s}%")) ||
                f.PracticeAreas.Any(p => EF.Functions.Like(p.Name, $"%{s}%")));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(f => f.City == filter.City);

        if (!string.IsNullOrWhiteSpace(filter.PracticeAreaSlug))
            query = query.Where(f => f.PracticeAreas.Any(p => p.Slug == filter.PracticeAreaSlug));

        if (filter.MinSalary.HasValue)
            query = query.Where(f => f.ArticlingSalary >= filter.MinSalary.Value);

        if (filter.MaxSalary.HasValue)
            query = query.Where(f => f.ArticlingSalary <= filter.MaxSalary.Value);

        if (filter.VerifiedOnly == true)
            query = query.Where(f => f.IsVerified);

        if (filter.PeerGroup.HasValue)
            query = query.Where(f => f.PeerGroup == filter.PeerGroup.Value);

        query = filter.Sort switch
        {
            FirmSort.SalaryDesc => query.OrderByDescending(f => f.ArticlingSalary).ThenBy(f => f.Name),
            FirmSort.SalaryAsc => query.OrderBy(f => f.ArticlingSalary).ThenBy(f => f.Name),
            FirmSort.RetentionDesc => query.OrderByDescending(f => f.RetentionRate).ThenBy(f => f.Name),
            FirmSort.RecentlyAdded => query.OrderByDescending(f => f.CreatedAtUtc),
            _ => query.OrderByDescending(f => f.IsFeaturedPartner).ThenBy(f => f.Name),
        };

        var totalCount = await query.CountAsync(ct);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 96);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FirmListItemDto(
                f.Id,
                f.Slug,
                f.Name,
                f.City,
                f.LogoUrl,
                f.ArticlingSalary,
                f.PeerGroup,
                f.IsVerified,
                f.IsFeaturedPartner,
                f.PracticeAreas
                    .Where(p => p.IsCorePractice)
                    .OrderBy(p => p.Name)
                    .Select(p => p.Name)
                    .ToList()))
            .ToListAsync(ct);

        return new PagedResult<FirmListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<FirmDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var firm = await _db.Firms
            .AsNoTracking()
            .Include(f => f.PracticeAreas)
            .Include(f => f.Perks)
            .Include(f => f.Reviews)
            .Include(f => f.Deadlines)
            .Where(f => f.Slug == slug && f.IsPublished)
            .OrderBy(f => f.Id)
            .FirstOrDefaultAsync(ct);

        if (firm is null) return null;

        var now = DateTime.UtcNow;

        return new FirmDetailDto(
            firm.Id,
            firm.Slug,
            firm.Name,
            firm.LegalName,
            firm.City,
            firm.Province,
            firm.LogoUrl,
            firm.WebsiteUrl,
            firm.ApplyUrl,
            firm.EarlyCareersUrl,
            firm.ArticlingSalary,
            firm.FirstYearAssociateSalary,
            firm.ClassSize,
            firm.RetentionRate,
            firm.BillableTarget,
            firm.OciDeadlineUtc,
            firm.ArticlingDeadlineUtc,
            firm.VibeSummary,
            firm.AiPolicy,
            firm.WorkLifeBalance,
            firm.OfficeCulture,
            firm.CompensationPhilosophy,
            firm.PeerGroup,
            firm.IsVerified,
            firm.IsFeaturedPartner,
            firm.PracticeAreas.OrderByDescending(p => p.IsCorePractice).ThenBy(p => p.Name).Select(p => p.Name).ToList(),
            firm.Perks.OrderBy(p => p.DisplayOrder).Select(p => new FirmPerkDto(p.Category, p.Name, p.Description)).ToList(),
            firm.Reviews.OrderByDescending(r => r.PublishedAtUtc).Select(r => new FirmReviewDto(r.Title, r.Body, r.AuthorAlias, r.IsAnonymous, r.IsEditorial, r.PublishedAtUtc)).ToList(),
            firm.Deadlines.OrderBy(d => d.DeadlineUtc).Select(d => new FirmDeadlineDto(d.Type, d.Label, d.DeadlineUtc, d.ApplyUrl, d.ComputeStatus(now), d.Notes)).ToList()
        );
    }

    public async Task<IReadOnlyList<string>> GetCitiesAsync(CancellationToken ct = default)
    {
        return await _db.Firms.AsNoTracking()
            .Where(f => f.IsPublished)
            .Select(f => f.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<(string Slug, string Name)>> GetPracticeAreasAsync(CancellationToken ct = default)
    {
        var rows = await _db.FirmPracticeAreas.AsNoTracking()
            .Select(p => new { p.Slug, p.Name })
            .Distinct()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
        return rows.Select(r => (r.Slug, r.Name)).ToList();
    }

    public async Task<IReadOnlyList<FirmListItemDto>> GetFeaturedAsync(int take, CancellationToken ct = default)
    {
        return await _db.Firms.AsNoTracking()
            .Where(f => f.IsPublished && f.IsFeaturedPartner)
            .OrderBy(f => f.Name)
            .Take(take)
            .Select(f => new FirmListItemDto(
                f.Id, f.Slug, f.Name, f.City, f.LogoUrl, f.ArticlingSalary,
                f.PeerGroup, f.IsVerified, f.IsFeaturedPartner,
                f.PracticeAreas.Where(p => p.IsCorePractice).OrderBy(p => p.Name).Select(p => p.Name).ToList()))
            .ToListAsync(ct);
    }
}
