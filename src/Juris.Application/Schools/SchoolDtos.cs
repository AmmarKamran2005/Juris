namespace Juris.Application.Schools;

public sealed record SchoolListItemDto(
    Guid Id,
    string Slug,
    string Name,
    string? ShortName,
    string City,
    string? LogoUrl,
    int? AnnualTuitionCad,
    decimal? MedianLsat,
    decimal? MedianGpa,
    decimal? EmploymentRateAt9MonthsPct,
    decimal? SevenSistersHireRatePct
);

public sealed record SchoolDetailDto(
    Guid Id,
    string Slug,
    string Name,
    string? ShortName,
    string City,
    string Province,
    string? LogoUrl,
    string? WebsiteUrl,
    int? AnnualTuitionCad,
    int? ClassSize,
    decimal? MedianLsat,
    decimal? MedianGpa,
    decimal? EmploymentRateAt9MonthsPct,
    decimal? SevenSistersHireRatePct,
    decimal? BarPassRatePct,
    string? AboutHtml,
    string? MootingNotes,
    string? ClinicNotes,
    string? PreLawPathwayNotes,
    string? CareerOutcomesNotes
);
