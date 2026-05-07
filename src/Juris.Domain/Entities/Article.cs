using Juris.Domain.Common;
using Juris.Domain.Enums;

namespace Juris.Domain.Entities;

public class Article : BaseEntity
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Excerpt { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public ArticleCategory Category { get; set; }
    public string AuthorName { get; set; } = "Juris Editorial";
    public string? AuthorUserId { get; set; }
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int ReadTimeMinutes { get; set; } = 4;
}
