using Juris.Domain.Enums;

namespace Juris.Application.Recruitment;

public interface IRecruitmentService
{
    Task<IReadOnlyList<RecruitmentDeadlineDto>> GetUpcomingAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<RecruitmentDeadlineDto>> ListAsync(DeadlineType? type, DeadlineStatus? status, CancellationToken ct = default);
}
