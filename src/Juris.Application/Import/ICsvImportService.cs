namespace Juris.Application.Import;

public interface ICsvImportService
{
    Task<ImportResult> ImportFirmsAsync(Stream csvStream, bool overwriteExisting, CancellationToken ct = default);
    Task<ImportResult> ImportSchoolsAsync(Stream csvStream, bool overwriteExisting, CancellationToken ct = default);
    Task<ImportResult> ImportArticlesAsync(Stream csvStream, bool overwriteExisting, CancellationToken ct = default);
}
