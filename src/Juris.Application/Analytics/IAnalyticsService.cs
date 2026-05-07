namespace Juris.Application.Analytics;

public interface IAnalyticsService
{
    Task TrackAsync(AnalyticsEventInput input, CancellationToken ct = default);
    Task<FirmAnalyticsDashboardDto?> GetFirmDashboardAsync(Guid firmId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<GlobalAnalyticsSnapshotDto> GetGlobalSnapshotAsync(CancellationToken ct = default);
}
