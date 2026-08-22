using EverestFlix.Domain.Constants;
using EverestFlix.Domain.Entities;
using EverestFlix.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Data;

public static class DataSeeder
{
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
            logger.LogWarning("Seed:CreatorEmail or Seed:CreatorPassword missing. Skipping seed.");
            return;
        }

        var creator = await userManager.FindByEmailAsync(creatorEmail);
        if (creator is null)
        {
            creator = new ApplicationUser
            {
                UserName       = creatorEmail,
                Email          = creatorEmail,
                EmailConfirmed = true,
                FullName       = creatorName,
                CreatedAt      = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(creator, creatorPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed Creator: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(creator, Roles.Creator);
            logger.LogInformation("Seeded Creator account: {Email}", creatorEmail);
        }

        if (!await db.Videos.AnyAsync())
        {
            db.Videos.AddRange(
                new Video
                {
                    Title       = "Welcome to EverestFlix",
                    Description = "A brief tour of the EverestFlix platform.",
                    Publisher   = "EverestFlix",
                    Producer    = "EverestFlix Studio",
                    Genre       = "Introduction",
                    AgeRating   = AgeRating.U,
                    VideoUrl    = "/uploads/videos/sample-welcome.mp4",
                    CreatorId   = creator.Id,
                    CreatedAt   = DateTime.UtcNow.AddMinutes(-30),
                    IsPublished = true
                },
                new Video
                {
                    Title       = "Cloud Fundamentals in 60 Seconds",
                    Description = "A whistle-stop tour of cloud-native architecture.",
                    Publisher   = "EverestFlix Learning",
                    Producer    = "Development Creator",
                    Genre       = "Education",
                    AgeRating   = AgeRating.PG,
                    VideoUrl    = "/uploads/videos/sample-cloud.mp4",
                    CreatorId   = creator.Id,
                    CreatedAt   = DateTime.UtcNow.AddMinutes(-15),
                    IsPublished = true
                });

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded 2 sample videos.");
        }
    }
}