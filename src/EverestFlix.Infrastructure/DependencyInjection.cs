using EverestFlix.Domain.Entities;
using EverestFlix.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EverestFlix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration.");

        services.AddDbContext<EverestFlixDbContext>(options =>
            options.UseSqlite(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Password policy — reasonable defaults for coursework
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<EverestFlixDbContext>();
            //.AddSignInManager()
            //.AddDefaultTokenProviders();

        return services;
    }
}