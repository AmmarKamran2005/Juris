using System.Globalization;
using System.Text;
using CsvHelper;
using Juris.Application.Common;
using Juris.Domain.Entities;
using Juris.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juris.Web.Infrastructure;

/// <summary>
/// Admin-only utility endpoints — CSV template downloads, full data exports,
/// .ics calendar export for recruitment deadlines.
/// </summary>
public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminUtilityEndpoints(this IEndpointRouteBuilder routes)
    {
        var admin = routes.MapGroup("/admin").RequireAuthorization("RequireAdmin");

        admin.MapGet("/import/template", (string entity) =>
        {
            string csv = entity?.ToLowerInvariant() switch
            {
                "firms" => "Slug,Name,LegalName,City,Province,LogoUrl,WebsiteUrl,ApplyUrl,EarlyCareersUrl,ArticlingSalary,FirstYearAssociateSalary,ClassSize,RetentionRate,BillableTarget,VibeSummary,AiPolicy,WorkLifeBalance,OfficeCulture,PeerGroup,PracticeAreas,IsVerified,IsFeaturedPartner,IsPublished,OciDeadlineUtc,ArticlingDeadlineUtc\n"
                       + "example-firm,Example Firm LLP,Example Firm Limited Liability Partnership,Toronto,Ontario,/img/firms/example.png,https://example.com,https://careers.example.com/apply,https://careers.example.com,115000,145000,28,80,1750,A pillar of the Bay Street market.,Generative AI is permitted with partner review.,1750 hours target with strict weekend protection.,Black-tie firm dinners with structured mentorship.,SevenSisters,M&A|Tax|Litigation,true,true,true,2026-09-15T17:00:00Z,2026-11-01T17:00:00Z\n",

                "schools" => "Slug,Name,ShortName,City,Province,LogoUrl,WebsiteUrl,AnnualTuitionCad,ClassSize,MedianLsat,MedianGpa,EmploymentRateAt9MonthsPct,SevenSistersHireRatePct,BarPassRatePct,AboutHtml,MootingNotes,ClinicNotes,PreLawPathwayNotes,CareerOutcomesNotes,IsPublished\n"
                       + "example-law,Example University Faculty of Law,Example,Toronto,Ontario,/img/schools/example.png,https://law.example.edu,32000,200,164,3.80,93,30,94,<p>Strong national platform.</p>,Top moot programme.,Three full-time clinics.,Strong intake from social-science backgrounds.,Top median starting salaries on Bay Street.,true\n",

                "articles" => "Slug,Title,Subtitle,Excerpt,BodyHtml,FeaturedImageUrl,Category,AuthorName,PublishedAtUtc,ReadTimeMinutes,IsPublished,IsFeatured\n"
                       + "example-article,Example Article Title,An optional subtitle,A short summary for the card view.,<p>Body in HTML.</p>,/img/articles/example.png,Careers,Juris Editorial,2026-05-01T12:00:00Z,5,true,false\n",

                _ => "error,unknown_entity\n"
            };
            var bytes = Encoding.UTF8.GetBytes(csv);
            return Results.File(bytes, "text/csv; charset=utf-8", $"juris-{entity}-template.csv");
        });

        admin.MapGet("/export/firms.csv", async (IApplicationDbContext db, CancellationToken ct) =>
        {
            var firms = await db.Firms.AsNoTracking()
                .Include(f => f.PracticeAreas)
                .OrderBy(f => f.Name).ToListAsync(ct);

            using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, leaveOpen: true);
            await using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture, leaveOpen: true);
            csv.WriteHeader<Juris.Application.Import.FirmCsvRow>();
            await csv.NextRecordAsync();
            foreach (var f in firms)
            {
                csv.WriteRecord(new Juris.Application.Import.FirmCsvRow
                {
                    Slug = f.Slug,
                    Name = f.Name,
                    LegalName = f.LegalName,
                    City = f.City,
                    Province = f.Province,
                    LogoUrl = f.LogoUrl,
                    WebsiteUrl = f.WebsiteUrl,
                    ApplyUrl = f.ApplyUrl,
                    EarlyCareersUrl = f.EarlyCareersUrl,
                    ArticlingSalary = f.ArticlingSalary,
                    FirstYearAssociateSalary = f.FirstYearAssociateSalary,
                    ClassSize = f.ClassSize,
                    RetentionRate = f.RetentionRate,
                    BillableTarget = f.BillableTarget,
                    VibeSummary = f.VibeSummary,
                    AiPolicy = f.AiPolicy,
                    WorkLifeBalance = f.WorkLifeBalance,
                    OfficeCulture = f.OfficeCulture,
                    PeerGroup = f.PeerGroup.ToString(),
                    PracticeAreas = string.Join('|', f.PracticeAreas.Select(p => p.Name)),
                    IsVerified = f.IsVerified,
                    IsFeaturedPartner = f.IsFeaturedPartner,
                    IsPublished = f.IsPublished,
                    OciDeadlineUtc = f.OciDeadlineUtc,
                    ArticlingDeadlineUtc = f.ArticlingDeadlineUtc,
                });
                await csv.NextRecordAsync();
            }
            await csv.FlushAsync();
            await sw.FlushAsync();
            return Results.File(ms.ToArray(), "text/csv; charset=utf-8", "juris-firms.csv");
        });

        admin.MapGet("/export/subscribers.csv", async (IApplicationDbContext db, CancellationToken ct) =>
        {
            var rows = await db.NewsletterSubscribers.AsNoTracking()
                .OrderBy(s => s.CreatedAtUtc).ToListAsync(ct);

            var sb = new StringBuilder();
            sb.AppendLine("Email,University,YearOfStudy,Source,IsConfirmed,UnsubscribedAtUtc,CreatedAtUtc");
            foreach (var s in rows)
            {
                sb.Append(Csv(s.Email)).Append(',')
                  .Append(Csv(s.University ?? "")).Append(',')
                  .Append(s.YearOfStudy.ToString()).Append(',')
                  .Append(Csv(s.Source)).Append(',')
                  .Append(s.IsConfirmed).Append(',')
                  .Append(s.UnsubscribedAtUtc?.ToString("o") ?? "").Append(',')
                  .Append(s.CreatedAtUtc.ToString("o")).Append('\n');
            }
            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", "juris-subscribers.csv");
        });

        admin.MapGet("/export/analytics.csv", async (IApplicationDbContext db, DateTime? from, DateTime? to, CancellationToken ct) =>
        {
            var fromUtc = (from ?? DateTime.UtcNow.AddDays(-30));
            var toUtc = (to ?? DateTime.UtcNow);

            var rows = await db.AnalyticsEvents.AsNoTracking()
                .Where(e => e.TimestampUtc >= fromUtc && e.TimestampUtc < toUtc)
                .OrderBy(e => e.TimestampUtc)
                .ToListAsync(ct);

            var sb = new StringBuilder();
            sb.AppendLine("TimestampUtc,Type,FirmId,SchoolId,ArticleId,Path,UniversityAffiliation,YearOfStudy,SearchTerm,SectionKey,DwellSeconds,SessionId");
            foreach (var e in rows)
            {
                sb.Append(e.TimestampUtc.ToString("o")).Append(',')
                  .Append(e.Type).Append(',')
                  .Append(e.FirmId).Append(',')
                  .Append(e.SchoolId).Append(',')
                  .Append(e.ArticleId).Append(',')
                  .Append(Csv(e.Path)).Append(',')
                  .Append(Csv(e.UniversityAffiliation ?? "")).Append(',')
                  .Append(e.YearOfStudy).Append(',')
                  .Append(Csv(e.SearchTerm ?? "")).Append(',')
                  .Append(Csv(e.SectionKey ?? "")).Append(',')
                  .Append(e.DwellSeconds?.ToString() ?? "").Append(',')
                  .Append(e.SessionId).Append('\n');
            }
            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", $"juris-analytics-{fromUtc:yyyyMMdd}-{toUtc:yyyyMMdd}.csv");
        });

        // ICS calendar feed of upcoming firm deadlines (publicly accessible — students subscribe)
        routes.MapGet("/recruitment/feed.ics", async (IApplicationDbContext db, Guid? firmId, CancellationToken ct) =>
        {
            var now = DateTime.UtcNow;
            var query = db.FirmDeadlines.AsNoTracking()
                .Include(d => d.Firm)
                .Where(d => d.Firm!.IsPublished && d.DeadlineUtc >= now);
            if (firmId.HasValue) query = query.Where(d => d.FirmId == firmId.Value);

            var rows = await query.OrderBy(d => d.DeadlineUtc).Take(500).ToListAsync(ct);

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//Juris//Recruitment Tracker//EN");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");
            sb.AppendLine("X-WR-CALNAME:Juris — Recruitment Deadlines");
            sb.AppendLine("X-WR-TIMEZONE:UTC");

            foreach (var d in rows)
            {
                var uid = $"{d.Id}@juris.local";
                var dt = d.DeadlineUtc.ToString("yyyyMMddTHHmmssZ");
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{uid}");
                sb.AppendLine($"DTSTAMP:{now:yyyyMMddTHHmmssZ}");
                sb.AppendLine($"DTSTART:{dt}");
                sb.AppendLine($"DTEND:{dt}");
                sb.AppendLine($"SUMMARY:{IcsEscape(d.Firm!.Name + " — " + d.Label)}");
                sb.AppendLine($"DESCRIPTION:{IcsEscape($"{d.Type} deadline for {d.Firm!.Name}. Apply: {d.ApplyUrl ?? "—"}")}");
                if (!string.IsNullOrEmpty(d.ApplyUrl)) sb.AppendLine($"URL:{d.ApplyUrl}");
                sb.AppendLine("END:VEVENT");
            }
            sb.AppendLine("END:VCALENDAR");

            return Results.File(Encoding.UTF8.GetBytes(sb.ToString()), "text/calendar; charset=utf-8", "juris-recruitment.ics");
        }).AllowAnonymous();

        return routes;
    }

    private static string Csv(string v) =>
        v.Contains(',') || v.Contains('"') || v.Contains('\n')
            ? "\"" + v.Replace("\"", "\"\"") + "\""
            : v;

    private static string IcsEscape(string v) =>
        v.Replace("\\", "\\\\")
         .Replace(",", "\\,")
         .Replace(";", "\\;")
         .Replace("\r\n", "\\n")
         .Replace("\n", "\\n");
}
