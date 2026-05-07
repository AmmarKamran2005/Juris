namespace Juris.Application.Search;

public interface ISearchService
{
    Task<SearchResponseDto> SearchAsync(string query, int perCategoryLimit = 5, CancellationToken ct = default);
}
