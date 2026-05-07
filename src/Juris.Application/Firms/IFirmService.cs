using Juris.Application.Common;

namespace Juris.Application.Firms;

public interface IFirmService
{
    Task<PagedResult<FirmListItemDto>> SearchAsync(FirmFilter filter, CancellationToken ct = default);
    Task<FirmDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCitiesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<(string Slug, string Name)>> GetPracticeAreasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FirmListItemDto>> GetFeaturedAsync(int take, CancellationToken ct = default);
}
