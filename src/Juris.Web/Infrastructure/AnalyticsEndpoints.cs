using System.Security.Cryptography;
using System.Text;
using Juris.Application.Analytics;
using Juris.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Juris.Web.Infrastructure;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/analytics").WithTags("Analytics");

        group.MapPost("/track", async (
            [FromBody] TrackEventPayload payload,
            HttpContext http,
            IAnalyticsService analytics,
            CancellationToken ct) =>
        {
            if (payload is null) return Results.BadRequest();

            if (!Enum.TryParse<AnalyticsEventType>(payload.Type, ignoreCase: true, out var type))
                return Results.BadRequest($"Unknown event type '{payload.Type}'.");

            var year = YearOfStudy.Unknown;
            if (!string.IsNullOrEmpty(payload.YearOfStudy))
                Enum.TryParse(payload.YearOfStudy, ignoreCase: true, out year);

            var ipHash = HashIp(http.Connection.RemoteIpAddress?.ToString());
            var userId = http.User?.Identity?.IsAuthenticated == true
                ? http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;

            await analytics.TrackAsync(new AnalyticsEventInput(
                Type: type,
                FirmId: TryParseGuid(payload.FirmId),
                SchoolId: TryParseGuid(payload.SchoolId),
                ArticleId: TryParseGuid(payload.ArticleId),
                UserId: userId,
                SessionId: TryParseGuid(payload.SessionId),
                UniversityAffiliation: payload.University,
                YearOfStudy: year,
                SearchTerm: payload.SearchTerm,
                SectionKey: payload.SectionKey,
                DwellSeconds: payload.DwellSeconds,
                Path: payload.Path ?? "/",
                Referrer: payload.Referrer,
                UserAgent: http.Request.Headers.UserAgent.ToString(),
                IpHash: ipHash,
                MetadataJson: payload.MetadataJson
            ), ct);

            return Results.NoContent();
        }).AllowAnonymous().RequireRateLimiting("analytics-ingest");

        return routes;
    }

    private static Guid? TryParseGuid(string? value)
        => Guid.TryParse(value, out var g) && g != Guid.Empty ? g : null;

    private static string? HashIp(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return null;
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(ip + "|juris-salt"));
        return Convert.ToHexString(bytes)[..16];
    }
}

public class TrackEventPayload
{
    public string Type { get; set; } = "PageView";
    public string? FirmId { get; set; }
    public string? SchoolId { get; set; }
    public string? ArticleId { get; set; }
    public string? SessionId { get; set; }
    public string? University { get; set; }
    public string? YearOfStudy { get; set; }
    public string? SearchTerm { get; set; }
    public string? SectionKey { get; set; }
    public int? DwellSeconds { get; set; }
    public string? Path { get; set; }
    public string? Referrer { get; set; }
    public string? MetadataJson { get; set; }
}
