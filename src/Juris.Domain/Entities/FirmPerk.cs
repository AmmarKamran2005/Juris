using Juris.Domain.Common;

namespace Juris.Domain.Entities;

public class FirmPerk : BaseEntity
{
    public Guid FirmId { get; set; }
    public Firm? Firm { get; set; }

    public string Category { get; set; } = string.Empty; // e.g., "AI Training", "Wellness", "Hybrid Work"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
