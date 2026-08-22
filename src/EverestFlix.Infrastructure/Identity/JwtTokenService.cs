using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EverestFlix.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var issuer   = _configuration["Jwt:Issuer"]   ?? throw new InvalidOperationException("Jwt:Issuer missing.");
        var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing.");
        var key      = _configuration["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key missing.");
        var lifetime = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "120");

        var expiresAt = DateTime.UtcNow.AddMinutes(lifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier,     user.Id),
            new(ClaimTypes.Name,               user.UserName ?? user.Email ?? string.Empty),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var signingKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}