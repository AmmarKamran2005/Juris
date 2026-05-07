using Juris.Domain.Enums;

namespace Juris.Application.Firms;

public sealed record FirmListItemDto(
    Guid Id,
    string Slug,
    string Name,
    string City,
    string? LogoUrl,
    int? ArticlingSalary,
    PeerGroup PeerGroup,
    bool IsVerified,
    bool IsFeaturedPartner,
    IReadOnlyList<string> PracticeAreaTags
);

public sealed record FirmFilter(
    string? Search = null,
    string? City = null,
    string? PracticeAreaSlug = null,
    int? MinSalary = null,
    int? MaxSalary = null,
    bool? VerifiedOnly = null,
    PeerGroup? PeerGroup = null,
    int Page = 1,
    int PageSize = 24,
    FirmSort Sort = FirmSort.NameAsc
);

public enum FirmSort
{
    NameAsc = 0,
    SalaryDesc = 1,
    SalaryAsc = 2,
    RetentionDesc = 3,
    RecentlyAdded = 4
}

public sealed record FirmPerkDto(string Category, string Name, string? Description);
public sealed record FirmReviewDto(string Title, string Body, string? AuthorAlias, bool IsAnonymous, bool IsEditorial, DateTime PublishedAtUtc);
public sealed record FirmDeadlineDto(DeadlineType Type, string Label, DateTime DeadlineUtc, string? ApplyUrl, DeadlineStatus Status, string? Notes);

public sealed record FirmDetailDto(
    Guid Id,
    string Slug,
    string Name,
    string? LegalName,
    string City,
    string Province,
    string? LogoUrl,
    string? WebsiteUrl,
    string? ApplyUrl,
    string? EarlyCareersUrl,
    int? ArticlingSalary,
    int? FirstYearAssociateSalary,
    int? ClassSize,
    decimal? RetentionRate,
    int? BillableTarget,
    DateTime? OciDeadlineUtc,
    DateTime? ArticlingDeadlineUtc,
    string? VibeSummary,
    string? AiPolicy,
    string? WorkLifeBalance,
    string? OfficeCulture,
    string? CompensationPhilosophy,
    PeerGroup PeerGroup,
    bool IsVerified,
    bool IsFeaturedPartner,
    IReadOnlyList<string> PracticeAreas,
    IReadOnlyList<FirmPerkDto> Perks,
    IReadOnlyList<FirmReviewDto> Reviews,
    IReadOnlyList<FirmDeadlineDto> Deadlines
);
