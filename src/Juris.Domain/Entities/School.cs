using Juris.Domain.Common;

namespace Juris.Domain.Entities;

public class School : BaseEntity
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = "Ontario";
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    // Comparison Matrix data
    public int? AnnualTuitionCad { get; set; }
    public int? ClassSize { get; set; }
    public decimal? MedianLsat { get; set; }
    public decimal? MedianGpa { get; set; }
    public decimal? EmploymentRateAt9MonthsPct { get; set; }
    public decimal? SevenSistersHireRatePct { get; set; }
    public decimal? BarPassRatePct { get; set; }

    // Deep-dive content
    public string? AboutHtml { get; set; }
    public string? MootingNotes { get; set; }
    public string? ClinicNotes { get; set; }
    public string? PreLawPathwayNotes { get; set; }
    public string? CareerOutcomesNotes { get; set; }

    public bool IsPublished { get; set; } = true;
}
