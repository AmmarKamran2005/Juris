using Juris.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Search;

public class SearchService : ISearchService
{
    private readonly IApplicationDbContext _db;

    public SearchService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SearchResponseDto> SearchAsync(string query, int perCategoryLimit = 5, CancellationToken ct = default)
    {
        var q = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(q))
            return new SearchResponseDto(q, Array.Empty<SearchResultDto>(), Array.Empty<SearchResultDto>(), Array.Empty<SearchResultDto>());

        var pattern = $"%{q}%";

        var firms = await _db.Firms.AsNoTracking()
            .Where(f => f.IsPublished &&
                (EF.Functions.Like(f.Name, pattern) ||
                 EF.Functions.Like(f.City, pattern) ||
                 (f.VibeSummary != null && EF.Functions.Like(f.VibeSummary, pattern)) ||
                 f.PracticeAreas.Any(p => EF.Functions.Like(p.Name, pattern))))
            .OrderByDescending(f => f.IsFeaturedPartner)
            .ThenBy(f => f.Name)
            .Take(perCategoryLimit)
            .Select(f => new SearchResultDto(
                SearchResultKind.Firm, f.Slug, f.Name, f.City + " · Articling " +
                (f.ArticlingSalary.HasValue ? "$" + f.ArticlingSalary.ToString() : "TBA"),
                "/firms/" + f.Slug))
            .ToListAsync(ct);

        var schools = await _db.Schools.AsNoTracking()
            .Where(s => s.IsPublished &&
                (EF.Functions.Like(s.Name, pattern) ||
                 (s.ShortName != null && EF.Functions.Like(s.ShortName, pattern)) ||
                 EF.Functions.Like(s.City, pattern)))
            .OrderBy(s => s.Name)
            .Take(perCategoryLimit)
            .Select(s => new SearchResultDto(
                SearchResultKind.School, s.Slug, s.Name, s.City,
                "/schools/" + s.Slug))
            .ToListAsync(ct);

        var articles = await _db.Articles.AsNoTracking()
            .Where(a => a.IsPublished &&
                (EF.Functions.Like(a.Title, pattern) ||
                 (a.Subtitle != null && EF.Functions.Like(a.Subtitle, pattern)) ||
                 (a.Excerpt != null && EF.Functions.Like(a.Excerpt, pattern))))
            .OrderByDescending(a => a.PublishedAtUtc)
            .Take(perCategoryLimit)
            .Select(a => new SearchResultDto(
                SearchResultKind.Article, a.Slug, a.Title, a.Subtitle,
                "/news/" + a.Slug))
            .ToListAsync(ct);

        return new SearchResponseDto(q, firms, schools, articles);
    }
}
