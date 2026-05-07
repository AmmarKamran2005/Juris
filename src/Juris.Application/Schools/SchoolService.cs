using Juris.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Schools;

public class SchoolService : ISchoolService
{
    private readonly IApplicationDbContext _db;

    public SchoolService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SchoolListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Schools.AsNoTracking()
            .Where(s => s.IsPublished)
            .OrderBy(s => s.Name)
            .Select(s => new SchoolListItemDto(
                s.Id, s.Slug, s.Name, s.ShortName, s.City, s.LogoUrl,
                s.AnnualTuitionCad, s.MedianLsat, s.MedianGpa,
                s.EmploymentRateAt9MonthsPct, s.SevenSistersHireRatePct))
            .ToListAsync(ct);
    }

    public async Task<SchoolDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var school = await _db.Schools.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == slug && s.IsPublished, ct);

        if (school is null) return null;

        return new SchoolDetailDto(
            school.Id, school.Slug, school.Name, school.ShortName,
            school.City, school.Province, school.LogoUrl, school.WebsiteUrl,
            school.AnnualTuitionCad, school.ClassSize,
            school.MedianLsat, school.MedianGpa,
            school.EmploymentRateAt9MonthsPct, school.SevenSistersHireRatePct, school.BarPassRatePct,
            school.AboutHtml, school.MootingNotes, school.ClinicNotes,
            school.PreLawPathwayNotes, school.CareerOutcomesNotes);
    }

    public async Task<IReadOnlyList<SchoolListItemDto>> GetForComparisonAsync(IEnumerable<string> slugs, CancellationToken ct = default)
    {
        var slugList = slugs.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (slugList.Count == 0) return Array.Empty<SchoolListItemDto>();

        return await _db.Schools.AsNoTracking()
            .Where(s => s.IsPublished && slugList.Contains(s.Slug))
            .OrderBy(s => s.Name)
            .Select(s => new SchoolListItemDto(
                s.Id, s.Slug, s.Name, s.ShortName, s.City, s.LogoUrl,
                s.AnnualTuitionCad, s.MedianLsat, s.MedianGpa,
                s.EmploymentRateAt9MonthsPct, s.SevenSistersHireRatePct))
            .ToListAsync(ct);
    }
}
