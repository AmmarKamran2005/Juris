using Juris.Domain.Common;

namespace Juris.Domain.Entities;

public class FirmReview : BaseEntity
{
    public Guid FirmId { get; set; }
    public Firm? Firm { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? AuthorAlias { get; set; }
    public bool IsAnonymous { get; set; } = true;
    public bool IsEditorial { get; set; }
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;
}
