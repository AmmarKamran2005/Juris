using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Juris.Application.Common;
using Juris.Domain.Entities;
using Juris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Import;

public class CsvImportService : ICsvImportService
{
    private readonly IApplicationDbContext _db;

    public CsvImportService(IApplicationDbContext db)
    {
        _db = db;
    }

    private static CsvConfiguration CsvConfig() => new(CultureInfo.InvariantCulture)
    {
        HeaderValidated = null,
        MissingFieldFound = null,
        TrimOptions = TrimOptions.Trim,
        BadDataFound = null,
    };

    public async Task<ImportResult> ImportFirmsAsync(Stream csvStream, bool overwriteExisting, CancellationToken ct = default)
    {
        var result = new ImportResult();
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, CsvConfig());

        List<FirmCsvRow> rows;
        try
        {
            rows = csv.GetRecords<FirmCsvRow>().ToList();
        }
        catch (Exception ex)
        {
            result.Errors++;
            result.ErrorMessages.Add("CSV parse failure: " + ex.Message);
            return result;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                if (string.IsNullOrWhiteSpace(row.Name) && string.IsNullOrWhiteSpace(row.Slug))
                {
                    result.Skipped++;
                    continue;
                }

                var slug = !string.IsNullOrWhiteSpace(row.Slug) ? row.Slug : Slug.From(row.Name!);
                if (string.IsNullOrWhiteSpace(slug))
                {
                    result.Skipped++;
                    continue;
                }

                var existing = await _db.Firms.Include(f => f.PracticeAreas)
                    .FirstOrDefaultAsync(f => f.Slug == slug, ct);

                if (existing is null)
                {
                    var firm = new Firm { Slug = slug };
                    Apply(row, firm);
                    UpdatePracticeAreas(firm, row.PracticeAreas);
                    _db.Firms.Add(firm);
                    result.Created++;
                }
                else if (overwriteExisting)
                {
                    Apply(row, existing);
                    UpdatePracticeAreas(existing, row.PracticeAreas);
                    existing.UpdatedAtUtc = DateTime.UtcNow;
                    result.Updated++;
                }
                else
                {
                    result.Skipped++;
                }
            }
            catch (Exception ex)
            {
                result.Errors++;
                result.ErrorMessages.Add($"Row {i + 2} ({row.Name ?? row.Slug ?? "?"}): {ex.Message}");
            }
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<ImportResult> ImportSchoolsAsync(Stream csvStream, bool overwriteExisting, CancellationToken ct = default)
    {
        var result = new ImportResult();
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, CsvConfig());

        List<SchoolCsvRow> rows;
        try { rows = csv.GetRecords<SchoolCsvRow>().ToList(); }
        catch (Exception ex)
        {
            result.Errors++;
            result.ErrorMessages.Add("CSV parse failure: " + ex.Message);
            return result;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                if (string.IsNullOrWhiteSpace(row.Name) && string.IsNullOrWhiteSpace(row.Slug))
                { result.Skipped++; continue; }

                var slug = !string.IsNullOrWhiteSpace(row.Slug) ? row.Slug : Slug.From(row.Name!);
                var existing = await _db.Schools.FirstOrDefaultAsync(s => s.Slug == slug, ct);

                if (existing is null)
                {
                    var s = new School { Slug = slug };
                    Apply(row, s);
                    _db.Schools.Add(s);
                    result.Created++;
                }
                else if (overwriteExisting)
                {
                    Apply(row, existing);
                    existing.UpdatedAtUtc = DateTime.UtcNow;
                    result.Updated++;
                }
                else result.Skipped++;
            }
            catch (Exception ex)
            {
                result.Errors++;
                result.ErrorMessages.Add($"Row {i + 2} ({row.Name ?? row.Slug ?? "?"}): {ex.Message}");
            }
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<ImportResult> ImportArticlesAsync(Stream csvStream, bool overwriteExisting, CancellationToken ct = default)
    {
        var result = new ImportResult();
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, CsvConfig());

        List<ArticleCsvRow> rows;
        try { rows = csv.GetRecords<ArticleCsvRow>().ToList(); }
        catch (Exception ex)
        {
            result.Errors++;
            result.ErrorMessages.Add("CSV parse failure: " + ex.Message);
            return result;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                if (string.IsNullOrWhiteSpace(row.Title) && string.IsNullOrWhiteSpace(row.Slug))
                { result.Skipped++; continue; }

                var slug = !string.IsNullOrWhiteSpace(row.Slug) ? row.Slug : Slug.From(row.Title!);
                var existing = await _db.Articles.FirstOrDefaultAsync(a => a.Slug == slug, ct);

                if (existing is null)
                {
                    var a = new Article { Slug = slug, BodyHtml = "" };
                    Apply(row, a);
                    _db.Articles.Add(a);
                    result.Created++;
                }
                else if (overwriteExisting)
                {
                    Apply(row, existing);
                    existing.UpdatedAtUtc = DateTime.UtcNow;
                    result.Updated++;
                }
                else result.Skipped++;
            }
            catch (Exception ex)
            {
                result.Errors++;
                result.ErrorMessages.Add($"Row {i + 2} ({row.Title ?? row.Slug ?? "?"}): {ex.Message}");
            }
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }

    // ---------- mappers ----------
    private static void Apply(FirmCsvRow row, Firm f)
    {
        if (!string.IsNullOrWhiteSpace(row.Name)) f.Name = row.Name!;
        if (row.LegalName is not null) f.LegalName = row.LegalName;
        if (!string.IsNullOrWhiteSpace(row.City)) f.City = row.City!;
        if (!string.IsNullOrWhiteSpace(row.Province)) f.Province = row.Province!;
        if (row.LogoUrl is not null) f.LogoUrl = row.LogoUrl;
        if (row.WebsiteUrl is not null) f.WebsiteUrl = row.WebsiteUrl;
        if (row.ApplyUrl is not null) f.ApplyUrl = row.ApplyUrl;
        if (row.EarlyCareersUrl is not null) f.EarlyCareersUrl = row.EarlyCareersUrl;
        if (row.ArticlingSalary.HasValue) f.ArticlingSalary = row.ArticlingSalary;
        if (row.FirstYearAssociateSalary.HasValue) f.FirstYearAssociateSalary = row.FirstYearAssociateSalary;
        if (row.ClassSize.HasValue) f.ClassSize = row.ClassSize;
        if (row.RetentionRate.HasValue) f.RetentionRate = row.RetentionRate;
        if (row.BillableTarget.HasValue) f.BillableTarget = row.BillableTarget;
        if (row.VibeSummary is not null) f.VibeSummary = row.VibeSummary;
        if (row.AiPolicy is not null) f.AiPolicy = row.AiPolicy;
        if (row.WorkLifeBalance is not null) f.WorkLifeBalance = row.WorkLifeBalance;
        if (row.OfficeCulture is not null) f.OfficeCulture = row.OfficeCulture;
        if (!string.IsNullOrWhiteSpace(row.PeerGroup) &&
            Enum.TryParse<PeerGroup>(row.PeerGroup, ignoreCase: true, out var pg))
            f.PeerGroup = pg;
        if (row.IsVerified.HasValue) f.IsVerified = row.IsVerified.Value;
        if (row.IsFeaturedPartner.HasValue) f.IsFeaturedPartner = row.IsFeaturedPartner.Value;
        if (row.IsPublished.HasValue) f.IsPublished = row.IsPublished.Value;
        if (row.OciDeadlineUtc.HasValue) f.OciDeadlineUtc = DateTime.SpecifyKind(row.OciDeadlineUtc.Value, DateTimeKind.Utc);
        if (row.ArticlingDeadlineUtc.HasValue) f.ArticlingDeadlineUtc = DateTime.SpecifyKind(row.ArticlingDeadlineUtc.Value, DateTimeKind.Utc);
    }

    private static void UpdatePracticeAreas(Firm firm, string? pipeList)
    {
        if (string.IsNullOrWhiteSpace(pipeList)) return;

        var names = pipeList.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .ToList();
        if (names.Count == 0) return;

        firm.PracticeAreas.Clear();
        for (int i = 0; i < names.Count; i++)
        {
            firm.PracticeAreas.Add(new FirmPracticeArea
            {
                Name = names[i],
                Slug = Slug.From(names[i]),
                IsCorePractice = i < 3,
            });
        }
    }

    private static void Apply(SchoolCsvRow row, School s)
    {
        if (!string.IsNullOrWhiteSpace(row.Name)) s.Name = row.Name!;
        if (row.ShortName is not null) s.ShortName = row.ShortName;
        if (!string.IsNullOrWhiteSpace(row.City)) s.City = row.City!;
        if (!string.IsNullOrWhiteSpace(row.Province)) s.Province = row.Province!;
        if (row.LogoUrl is not null) s.LogoUrl = row.LogoUrl;
        if (row.WebsiteUrl is not null) s.WebsiteUrl = row.WebsiteUrl;
        if (row.AnnualTuitionCad.HasValue) s.AnnualTuitionCad = row.AnnualTuitionCad;
        if (row.ClassSize.HasValue) s.ClassSize = row.ClassSize;
        if (row.MedianLsat.HasValue) s.MedianLsat = row.MedianLsat;
        if (row.MedianGpa.HasValue) s.MedianGpa = row.MedianGpa;
        if (row.EmploymentRateAt9MonthsPct.HasValue) s.EmploymentRateAt9MonthsPct = row.EmploymentRateAt9MonthsPct;
        if (row.SevenSistersHireRatePct.HasValue) s.SevenSistersHireRatePct = row.SevenSistersHireRatePct;
        if (row.BarPassRatePct.HasValue) s.BarPassRatePct = row.BarPassRatePct;
        if (row.AboutHtml is not null) s.AboutHtml = row.AboutHtml;
        if (row.MootingNotes is not null) s.MootingNotes = row.MootingNotes;
        if (row.ClinicNotes is not null) s.ClinicNotes = row.ClinicNotes;
        if (row.PreLawPathwayNotes is not null) s.PreLawPathwayNotes = row.PreLawPathwayNotes;
        if (row.CareerOutcomesNotes is not null) s.CareerOutcomesNotes = row.CareerOutcomesNotes;
        if (row.IsPublished.HasValue) s.IsPublished = row.IsPublished.Value;
    }

    private static void Apply(ArticleCsvRow row, Article a)
    {
        if (!string.IsNullOrWhiteSpace(row.Title)) a.Title = row.Title!;
        if (row.Subtitle is not null) a.Subtitle = row.Subtitle;
        if (row.Excerpt is not null) a.Excerpt = row.Excerpt;
        if (row.BodyHtml is not null) a.BodyHtml = row.BodyHtml;
        if (row.FeaturedImageUrl is not null) a.FeaturedImageUrl = row.FeaturedImageUrl;
        if (!string.IsNullOrWhiteSpace(row.Category) &&
            Enum.TryParse<ArticleCategory>(row.Category, ignoreCase: true, out var cat))
            a.Category = cat;
        if (row.AuthorName is not null) a.AuthorName = row.AuthorName;
        if (row.PublishedAtUtc.HasValue) a.PublishedAtUtc = DateTime.SpecifyKind(row.PublishedAtUtc.Value, DateTimeKind.Utc);
        if (row.ReadTimeMinutes.HasValue) a.ReadTimeMinutes = row.ReadTimeMinutes.Value;
        if (row.IsPublished.HasValue) a.IsPublished = row.IsPublished.Value;
        if (row.IsFeatured.HasValue) a.IsFeatured = row.IsFeatured.Value;
    }
}
