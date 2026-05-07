using Juris.Domain.Enums;

namespace Juris.Application.Recruitment;

public sealed record RecruitmentDeadlineDto(
    Guid FirmId,
    string FirmSlug,
    string FirmName,
    string City,
    string? LogoUrl,
    DeadlineType Type,
    string Label,
    DateTime DeadlineUtc,
    string? ApplyUrl,
    DeadlineStatus Status,
    int DaysRemaining,
    bool IsVerified
);
