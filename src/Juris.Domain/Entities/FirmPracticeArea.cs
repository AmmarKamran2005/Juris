using Juris.Domain.Common;

namespace Juris.Domain.Entities;

public class FirmPracticeArea : BaseEntity
{
    public Guid FirmId { get; set; }
    public Firm? Firm { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsCorePractice { get; set; }
}
