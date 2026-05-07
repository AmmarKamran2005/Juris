namespace Juris.Application.Search;

public enum SearchResultKind { Firm, School, Article }

public sealed record SearchResultDto(
    SearchResultKind Kind,
    string Slug,
    string Title,
    string? Subtitle,
    string Url
);

public sealed record SearchResponseDto(
    string Query,
    IReadOnlyList<SearchResultDto> Firms,
    IReadOnlyList<SearchResultDto> Schools,
    IReadOnlyList<SearchResultDto> Articles
);
