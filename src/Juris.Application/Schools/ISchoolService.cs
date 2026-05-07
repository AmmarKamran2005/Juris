namespace Juris.Application.Schools;

public interface ISchoolService
{
    Task<IReadOnlyList<SchoolListItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<SchoolDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<SchoolListItemDto>> GetForComparisonAsync(IEnumerable<string> slugs, CancellationToken ct = default);
}
