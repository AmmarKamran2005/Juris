using Juris.Domain.Common;
using Juris.Domain.Enums;

namespace Juris.Domain.Entities;

public class Firm : BaseEntity
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = "Ontario";
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ApplyUrl { get; set; }
    public string? EarlyCareersUrl { get; set; }

    // Hard Data — sidebar
    public int? ArticlingSalary { get; set; }
    public int? FirstYearAssociateSalary { get; set; }
    public int? ClassSize { get; set; }
    public decimal? RetentionRate { get; set; }
    public int? BillableTarget { get; set; }
    public DateTime? OciDeadlineUtc { get; set; }
    public DateTime? ArticlingDeadlineUtc { get; set; }

    // Vibe — main content
    public string? VibeSummary { get; set; }
    public string? AiPolicy { get; set; }
    public string? WorkLifeBalance { get; set; }
    public string? OfficeCulture { get; set; }
    public string? CompensationPhilosophy { get; set; }

    public PeerGroup PeerGroup { get; set; } = PeerGroup.MidLaw;

    public bool IsVerified { get; set; }
    public bool IsPublished { get; set; } = true;
    public bool IsFeaturedPartner { get; set; }

    public string? OwnerUserId { get; set; }

    public ICollection<FirmPracticeArea> PracticeAreas { get; set; } = new List<FirmPracticeArea>();
    public ICollection<FirmPerk> Perks { get; set; } = new List<FirmPerk>();
    public ICollection<FirmReview> Reviews { get; set; } = new List<FirmReview>();
    public ICollection<FirmDeadline> Deadlines { get; set; } = new List<FirmDeadline>();
}
