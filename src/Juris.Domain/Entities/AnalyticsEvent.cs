using Juris.Domain.Enums;

namespace Juris.Domain.Entities;

/// <summary>
/// Event-sourced analytics record. The "Analytics Engine" moat:
/// every meaningful interaction the student takes on the platform
/// gets a row here so HR dashboards can be aggregated on demand.
/// </summary>
public class AnalyticsEvent
{
    public long Id { get; set; }
    public AnalyticsEventType Type { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public Guid? FirmId { get; set; }
    public Firm? Firm { get; set; }
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
    public Guid? ArticleId { get; set; }
    public Article? Article { get; set; }

    public string? UserId { get; set; }
    public Guid SessionId { get; set; }

    public string? UniversityAffiliation { get; set; }
    public YearOfStudy YearOfStudy { get; set; } = YearOfStudy.Unknown;

    public string? SearchTerm { get; set; }
    public string? SectionKey { get; set; } // e.g. "hard-data", "vibe", "ai-policy"
    public int? DwellSeconds { get; set; }

    public string Path { get; set; } = string.Empty;
    public string? Referrer { get; set; }
    public string? UserAgent { get; set; }
    public string? IpHash { get; set; }
    public string? MetadataJson { get; set; }
}
