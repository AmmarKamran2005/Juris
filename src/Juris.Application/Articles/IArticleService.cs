using Juris.Domain.Enums;

namespace Juris.Application.Articles;

public interface IArticleService
{
    Task<IReadOnlyList<ArticleListItemDto>> GetTrendingAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<ArticleListItemDto>> ListAsync(ArticleCategory? category, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(ArticleCategory? category, CancellationToken ct = default);
    Task<ArticleDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
}
