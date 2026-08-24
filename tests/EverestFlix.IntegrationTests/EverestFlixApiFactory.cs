using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EverestFlix.IntegrationTests;


public sealed class EverestFlixApiFactory
    : WebApplicationFactory<Program>
{
    public const string CreatorEmail =
        "creator.integration@everestflix.local";

    public const string CreatorPassword =
        "CreatorTest#2026";


    public const string AdminEmail =
        "admin.integration@everestflix.local";

    public const string AdminPassword =
        "AdminTest#2026";


    private const string JwtIssuer =
        "EverestFlix.IntegrationTests";

    private const string JwtAudience =
        "EverestFlix.IntegrationTests.Client";

    private const string JwtKey =
        "integration-test-jwt-signing-key-everestflix-2026-abcdef1234567890";


    private readonly string _databasePath =
        Path.Combine(
            Path.GetTempPath(),
            $"everestflix-it-{Guid.NewGuid():N}.db");


    private readonly string _storageRoot =
        Path.Combine(
            Path.GetTempPath(),
            $"everestflix-it-videos-{Guid.NewGuid():N}");


    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            "Development");


        // ---------------------------------------------------------
        // Integration-test configuration
        // ---------------------------------------------------------

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                var values =
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            $"Data Source={_databasePath}",


                        ["Jwt:Issuer"] =
                            JwtIssuer,

                        ["Jwt:Audience"] =
                            JwtAudience,

                        ["Jwt:Key"] =
                            JwtKey,

                        ["Jwt:ExpiresInMinutes"] =
                            "60",


                        ["Storage:LocalRoot"] =
                            _storageRoot,

                        ["Storage:PublicBaseUrl"] =
                            "/uploads/videos",


                        ["Seed:CreatorEmail"] =
                            CreatorEmail,

                        ["Seed:CreatorPassword"] =
                            CreatorPassword,

                        ["Seed:CreatorFullName"] =
                            "Integration Test Creator",


                        ["Seed:AdminEmail"] =
                            AdminEmail,

                        ["Seed:AdminPassword"] =
                            AdminPassword,

                        ["Seed:AdminFullName"] =
                            "Integration Test Admin",


                        ["Cors:AllowedOrigins:0"] =
                            "http://localhost:5100"
                    };


                configuration.AddInMemoryCollection(
                    values);
            });


        // ---------------------------------------------------------
        // IMPORTANT:
        // Make JwtBearer validation use exactly the same issuer,
        // audience and signing key as JwtTokenService uses when
        // generating tokens inside the integration-test host.
        // ---------------------------------------------------------

        builder.ConfigureServices(
            services =>
            {
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuer =
                                    true,

                                ValidateAudience =
                                    true,

                                ValidateLifetime =
                                    true,

                                ValidateIssuerSigningKey =
                                    true,


                                ValidIssuer =
                                    JwtIssuer,

                                ValidAudience =
                                    JwtAudience,

                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(
                                            JwtKey)),


                                ClockSkew =
                                    TimeSpan.FromSeconds(30)
                            };
                    });
            });
    }


    protected override void Dispose(
        bool disposing)
    {
        base.Dispose(
            disposing);


        TryDeleteFile(
            _databasePath);

        TryDeleteFile(
            $"{_databasePath}-shm");

        TryDeleteFile(
            $"{_databasePath}-wal");


        try
        {
            if (Directory.Exists(
                    _storageRoot))
            {
                Directory.Delete(
                    _storageRoot,
                    recursive: true);
            }
        }
        catch
        {
            // Cleanup failure should never fail the test suite.
        }
    }


    private static void TryDeleteFile(
        string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(
                    path);
            }
        }
        catch
        {
            // Cleanup failure should never fail the test suite.
        }
    }
}