using Juris.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Juris.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? University { get; set; }
    public YearOfStudy YearOfStudy { get; set; } = YearOfStudy.Unknown;

    /// <summary>
    /// FirmId for users in the FirmHR role — links them to the firm
    /// they are authorised to manage in the Verified Portal.
    /// </summary>
    public Guid? FirmId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginUtc { get; set; }
}
