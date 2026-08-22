using EverestFlix.Domain.Constants;
using EverestFlix.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Data;

public static class DataSeeder
{
    /// <summary>
    /// Applies pending migrations, ensures roles exist, and seeds a development Creator account.
    /// Safe to call on every application startup — all operations are idempotent.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger        = services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
        var db            = services.GetRequiredService<EverestFlixDbContext>();
        var roleManager   = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager   = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied.");

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Seeded role: {Role}", role);
            }
        }

        var creatorEmail    = configuration["Seed:CreatorEmail"];
        var creatorPassword = configuration["Seed:CreatorPassword"];
        var creatorName     = configuration["Seed:CreatorFullName"] ?? "Development Creator";

        if (string.IsNullOrWhiteSpace(creatorEmail) || string.IsNullOrWhiteSpace(creatorPassword))
        {
            logger.LogWarning("Seed:CreatorEmail or Seed:CreatorPassword missing from config. Skipping Creator seed.");
            return;
        }

        if (await userManager.FindByEmailAsync(creatorEmail) is null)
        {
            var creator = new ApplicationUser
            {
                UserName       = creatorEmail,
                Email          = creatorEmail,
                EmailConfirmed = true,
                FullName       = creatorName,
                CreatedAt      = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(creator, creatorPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(creator, Roles.Creator);
                logger.LogInformation("Seeded Creator account: {Email}", creatorEmail);
            }
            else
            {
                logger.LogError("Failed to seed Creator: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}