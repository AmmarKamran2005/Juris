using Juris.Infrastructure.Identity;
using Juris.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Juris.Web.Infrastructure;

/// <summary>
/// Runs at startup: applies pending EF migrations, ensures the three
/// platform roles exist, ensures a default admin user exists, and
/// seeds placeholder firm/school/article data so the demo lights up.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("Migrations applied.");

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role {Role}", role);
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();
        var adminEmail = config["JurisAdmin:Email"] ?? "admin@juris.local";
        var adminPassword = config["JurisAdmin:Password"] ?? "ChangeMe!2026";
        var adminDisplayName = config["JurisAdmin:DisplayName"] ?? "Juris Admin";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = adminDisplayName,
            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }
            await userManager.AddToRoleAsync(admin, Roles.Admin);
            logger.LogInformation("Seeded default admin {Email}", adminEmail);
        }
        else if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        await SeedData.SeedAsync(db, logger);
    }
}
