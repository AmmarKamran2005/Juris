using Juris.Application.Common;
using Juris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Articles;

public class ArticleService : IArticleService
{
    private readonly IApplicationDbContext _db;

    public ArticleService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ArticleListItemDto>> GetTrendingAsync(int take, CancellationToken ct = default)
    {
        return await _db.Articles.AsNoTracking()
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.IsFeatured)
            .ThenByDescending(a => a.PublishedAtUtc)
            .Take(take)
            .Select(a => new ArticleListItemDto(
                a.Id, a.Slug, a.Title, a.Subtitle, a.Excerpt, a.FeaturedImageUrl,
                a.Category, a.AuthorName, a.PublishedAtUtc, a.ReadTimeMinutes, a.IsFeatured))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(ArticleCategory? category, CancellationToken ct = default)
    {
        var q = _db.Articles.AsNoTracking().Where(a => a.IsPublished);
        if (category.HasValue) q = q.Where(a => a.Category == category.Value);
        return q.CountAsync(ct);
    }

    public async Task<IReadOnlyList<ArticleListItemDto>> ListAsync(ArticleCategory? category, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Articles.AsNoTracking().Where(a => a.IsPublished);
        if (category.HasValue) q = q.Where(a => a.Category == category.Value);
        return await q
            .OrderByDescending(a => a.PublishedAtUtc)
            .Skip(Math.Max(0, page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleListItemDto(
                a.Id, a.Slug, a.Title, a.Subtitle, a.Excerpt, a.FeaturedImageUrl,
                a.Category, a.AuthorName, a.PublishedAtUtc, a.ReadTimeMinutes, a.IsFeatured))
            .ToListAsync(ct);
    }

    public async Task<ArticleDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var a = await _db.Articles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsPublished, ct);
        if (a is null) return null;
        return new ArticleDetailDto(
            a.Id, a.Slug, a.Title, a.Subtitle, a.Excerpt, a.BodyHtml, a.FeaturedImageUrl,
            a.Category, a.AuthorName, a.PublishedAtUtc, a.ReadTimeMinutes);
    }
}
