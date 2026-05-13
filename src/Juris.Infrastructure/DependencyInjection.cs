using Juris.Application.Common;
using Juris.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juris.Infrastructure;

public static partial class DependencyInjection
{
    /// <summary>
    /// Registers EF Core ApplicationDbContext using either SQL Server or PostgreSQL
    /// based on the connection string format (or an explicit "DatabaseProvider" config).
    /// Lets the same codebase run on Windows + SQL Server locally AND on Render with
    /// managed PostgreSQL without any code changes.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        // Render gives Postgres URLs as `postgres://user:pass@host:port/db` — translate
        // them to Npgsql's keyword=value form so callers can paste as-is.
        connectionString = NormalizePostgresUrl(connectionString);

        var provider = DetectProvider(configuration, connectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (provider)
            {
                case DbProvider.Postgres:
                    options.UseNpgsql(connectionString, npgsql =>
                    {
                        npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                    });
                    break;
                default:
                    options.UseSqlServer(connectionString, sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure(maxRetryCount: 3);
                        sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });
                    break;
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton(new DbProviderInfo(provider));

        return services;
    }

    public static DbProvider DetectProvider(IConfiguration configuration, string connectionString)
    {
        // Explicit override always wins
        var configured = configuration["DatabaseProvider"];
        if (!string.IsNullOrEmpty(configured) &&
            Enum.TryParse<DbProvider>(configured, ignoreCase: true, out var explicitProvider))
        {
            return explicitProvider;
        }

        // Otherwise, sniff from connection-string format
        var cs = connectionString.ToLowerInvariant();
        if (cs.StartsWith("postgres://") ||
            cs.StartsWith("postgresql://") ||
            cs.Contains("host=") ||
            cs.Contains("user id=postgres") ||
            cs.Contains("port=5432"))
        {
            return DbProvider.Postgres;
        }
        return DbProvider.SqlServer;
    }
}

public enum DbProvider
{
    SqlServer = 0,
    Postgres = 1,
}

/// <summary>Resolved database provider, available via DI.</summary>
public sealed record DbProviderInfo(DbProvider Provider);

public static partial class DependencyInjection
{
    /// <summary>
    /// Accepts Render-style URLs (postgres://user:pwd@host:port/db) and translates
    /// to Npgsql's expected keyword=value format. Leaves non-URL strings unchanged.
    /// </summary>
    public static string NormalizePostgresUrl(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        var lower = raw.TrimStart().ToLowerInvariant();
        if (!lower.StartsWith("postgres://") && !lower.StartsWith("postgresql://")) return raw;

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri)) return raw;

        var user = Uri.UnescapeDataString(uri.UserInfo.Split(':', 2)[0]);
        var pass = uri.UserInfo.Contains(':') ? Uri.UnescapeDataString(uri.UserInfo.Split(':', 2)[1]) : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        // SSL Mode=Require is what Render expects; TrustServerCertificate handles
        // their self-signed internal certs gracefully.
        return $"Host={host};Port={port};Username={user};Password={pass};Database={database};SSL Mode=Require;Trust Server Certificate=true;Pooling=true";
    }
}
