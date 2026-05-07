using Juris.Domain.Common;
using Juris.Domain.Enums;

namespace Juris.Domain.Entities;

public class NewsletterSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? University { get; set; }
    public YearOfStudy YearOfStudy { get; set; } = YearOfStudy.Unknown;
    public string Source { get; set; } = "homepage";
    public bool IsConfirmed { get; set; }
    public string? ConfirmationToken { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? UnsubscribedAtUtc { get; set; }
}
