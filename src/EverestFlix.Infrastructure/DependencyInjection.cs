using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Entities;
using EverestFlix.Infrastructure.Admin;
using EverestFlix.Infrastructure.Comments;
using EverestFlix.Infrastructure.Data;
using EverestFlix.Infrastructure.Identity;
using EverestFlix.Infrastructure.Ratings;
using EverestFlix.Infrastructure.Storage;
using EverestFlix.Infrastructure.Videos;
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
                options.Password.RequireDigit           = true;
                options.Password.RequiredLength         = 8;
                options.Password.RequireLowercase       = true;
                options.Password.RequireUppercase       = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<EverestFlixDbContext>();

        services.AddScoped<IJwtTokenService,     JwtTokenService>();
        services.AddScoped<IAuthService,         AuthService>();
        services.AddScoped<IVideoStorageService, LocalVideoStorageService>();
        services.AddScoped<IVideoService,        VideoService>();
        services.AddScoped<ICommentService,      CommentService>();
        services.AddScoped<IRatingService,       RatingService>();
        services.AddScoped<IAdminService,        AdminService>();

        return services;
    }
}