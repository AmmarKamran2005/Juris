namespace Juris.Application.Import;

public sealed class ImportResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}

/// <summary>
/// CSV-mapped Firm row. Header names are case-sensitive in CsvHelper by default;
/// our reader uses HeaderValidated=null + MissingFieldFound=null so partial rows are OK.
/// </summary>
public sealed class FirmCsvRow
{
    public string? Slug { get; set; }
    public string? Name { get; set; }
    public string? LegalName { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ApplyUrl { get; set; }
    public string? EarlyCareersUrl { get; set; }
    public int? ArticlingSalary { get; set; }
    public int? FirstYearAssociateSalary { get; set; }
    public int? ClassSize { get; set; }
    public decimal? RetentionRate { get; set; }
    public int? BillableTarget { get; set; }
    public string? VibeSummary { get; set; }
    public string? AiPolicy { get; set; }
    public string? WorkLifeBalance { get; set; }
    public string? OfficeCulture { get; set; }
    public string? PeerGroup { get; set; }
    public string? PracticeAreas { get; set; } // pipe-delimited list, e.g. "M&A|Tax|Litigation"
    public bool? IsVerified { get; set; }
    public bool? IsFeaturedPartner { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? OciDeadlineUtc { get; set; }
    public DateTime? ArticlingDeadlineUtc { get; set; }
}

public sealed class SchoolCsvRow
{
    public string? Slug { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? AnnualTuitionCad { get; set; }
    public int? ClassSize { get; set; }
    public decimal? MedianLsat { get; set; }
    public decimal? MedianGpa { get; set; }
    public decimal? EmploymentRateAt9MonthsPct { get; set; }
    public decimal? SevenSistersHireRatePct { get; set; }
    public decimal? BarPassRatePct { get; set; }
    public string? AboutHtml { get; set; }
    public string? MootingNotes { get; set; }
    public string? ClinicNotes { get; set; }
    public string? PreLawPathwayNotes { get; set; }
    public string? CareerOutcomesNotes { get; set; }
    public bool? IsPublished { get; set; }
}

public sealed class ArticleCsvRow
{
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Excerpt { get; set; }
    public string? BodyHtml { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? Category { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public int? ReadTimeMinutes { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsFeatured { get; set; }
}
