using System.Text;
using Juris.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Juris.Web.Infrastructure;

/// <summary>
/// Public, non-Razor endpoints — robots.txt, sitemap.xml, /health.
/// </summary>
public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/robots.txt", (HttpContext ctx) =>
        {
            var host = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            var body = $"User-agent: *\nAllow: /\nDisallow: /admin/\nDisallow: /portal/\nDisallow: /Account/\nSitemap: {host}/sitemap.xml\n";
            return Results.Text(body, "text/plain; charset=utf-8");
        }).AllowAnonymous();

        routes.MapGet("/sitemap.xml", async (IApplicationDbContext db, HttpContext ctx, CancellationToken ct) =>
        {
            var host = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            void Url(string path, DateTime? lastMod = null, double priority = 0.5)
            {
                sb.Append("<url><loc>").Append(host).Append(path).Append("</loc>");
                if (lastMod.HasValue) sb.Append("<lastmod>").Append(lastMod.Value.ToString("yyyy-MM-dd")).Append("</lastmod>");
                sb.Append("<priority>").Append(priority.ToString("0.0")).Append("</priority>");
                sb.AppendLine("</url>");
            }

            Url("/", null, 1.0);
            Url("/firms", null, 0.9);
            Url("/schools", null, 0.9);
            Url("/recruitment", null, 0.8);
            Url("/news", null, 0.8);
            Url("/verified", null, 0.6);
            Url("/about", null, 0.4);

            foreach (var f in await db.Firms.AsNoTracking().Where(x => x.IsPublished).Select(x => new { x.Slug, x.UpdatedAtUtc }).ToListAsync(ct))
                Url($"/firms/{f.Slug}", f.UpdatedAtUtc, 0.7);

            foreach (var s in await db.Schools.AsNoTracking().Where(x => x.IsPublished).Select(x => new { x.Slug, x.UpdatedAtUtc }).ToListAsync(ct))
                Url($"/schools/{s.Slug}", s.UpdatedAtUtc, 0.7);

            foreach (var a in await db.Articles.AsNoTracking().Where(x => x.IsPublished).Select(x => new { x.Slug, x.PublishedAtUtc }).ToListAsync(ct))
                Url($"/news/{a.Slug}", a.PublishedAtUtc, 0.6);

            sb.AppendLine("</urlset>");
            return Results.Text(sb.ToString(), "application/xml; charset=utf-8");
        }).AllowAnonymous();

        routes.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow })).AllowAnonymous();

        routes.MapGet("/health/ready", async (IApplicationDbContext db, CancellationToken ct) =>
        {
            try
            {
                await db.Firms.Take(1).CountAsync(ct);
                return Results.Ok(new { status = "ready", db = "ok" });
            }
            catch (Exception ex)
            {
                return Results.Json(new { status = "degraded", db = ex.Message }, statusCode: 503);
            }
        }).AllowAnonymous();

        return routes;
    }
}
