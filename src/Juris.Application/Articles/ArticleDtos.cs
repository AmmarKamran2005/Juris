using Juris.Domain.Enums;

namespace Juris.Application.Articles;

public sealed record ArticleListItemDto(
    Guid Id,
    string Slug,
    string Title,
    string? Subtitle,
    string? Excerpt,
    string? FeaturedImageUrl,
    ArticleCategory Category,
    string AuthorName,
    DateTime PublishedAtUtc,
    int ReadTimeMinutes,
    bool IsFeatured
);

public sealed record ArticleDetailDto(
    Guid Id,
    string Slug,
    string Title,
    string? Subtitle,
    string? Excerpt,
    string BodyHtml,
    string? FeaturedImageUrl,
    ArticleCategory Category,
    string AuthorName,
    DateTime PublishedAtUtc,
    int ReadTimeMinutes
);
