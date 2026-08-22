using EverestFlix.Domain.Entities;

namespace EverestFlix.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user, IEnumerable<string> roles);
}