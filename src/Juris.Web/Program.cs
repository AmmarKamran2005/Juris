using System.Threading.RateLimiting;
using Juris.Application;
using Juris.Infrastructure;
using Juris.Infrastructure.Identity;
using Juris.Infrastructure.Persistence;
using Juris.Web.Components;
using Juris.Web.Components.Account;
using Juris.Web.Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;

namespace Juris.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        // Infrastructure registers ApplicationDbContext (SQL Server) from configuration.
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdmin", p => p.RequireRole(Roles.Admin))
            .AddPolicy("RequireFirmHR", p => p.RequireRole(Roles.FirmHR, Roles.Admin));

        builder.Services.AddRateLimiter(options =>
        {
            // Global cap on /api/analytics/track per IP — protects the event ingestion
            // endpoint from spam without affecting normal student traffic.
            options.AddPolicy("analytics-ingest", httpContext =>
            {
                var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    });
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // Trust X-Forwarded-* headers from reverse proxy (Render, nginx, etc.)
        // so that scheme/host detection (e.g. for sitemap.xml + cookies) works
        // correctly behind a TLS-terminating edge.
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        var app = builder.Build();

        app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/404");

        // No app-level HTTPS redirect: Render / nginx / any reverse proxy
        // terminates TLS at the edge and forwards HTTP to the container.
        // ForwardedHeaders (above) reads X-Forwarded-Proto so the app still
        // sees `https` for URL generation (sitemap, OG canonical, etc).
        // Enabling HttpsRedirection here would just cause confusing
        // "Failed to determine the https port for redirect" warnings.

        app.UseStaticFiles();
        app.UseRateLimiter();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Juris.Web.Client._Imports).Assembly);

        app.MapAdditionalIdentityEndpoints();
        app.MapAnalyticsEndpoints();
        app.MapAdminUtilityEndpoints();
        app.MapPublicEndpoints();

        // Apply pending migrations and seed roles + default admin + sample data.
        await using (var scope = app.Services.CreateAsyncScope())
        {
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider);
        }

        app.Run();
    }
}
