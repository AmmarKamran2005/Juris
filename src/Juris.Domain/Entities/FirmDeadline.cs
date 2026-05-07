using Juris.Domain.Common;
using Juris.Domain.Enums;

namespace Juris.Domain.Entities;

public class FirmDeadline : BaseEntity
{
    public Guid FirmId { get; set; }
    public Firm? Firm { get; set; }

    public DeadlineType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime DeadlineUtc { get; set; }
    public string? ApplyUrl { get; set; }
    public string? Notes { get; set; }

    public DeadlineStatus ComputeStatus(DateTime nowUtc)
    {
        var diff = DeadlineUtc - nowUtc;
        if (diff.TotalMinutes <= 0) return DeadlineStatus.Closed;
        if (diff.TotalDays <= 7) return DeadlineStatus.ClosingSoon;
        return DeadlineStatus.Open;
    }
}
