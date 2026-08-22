using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Auth;

namespace EverestFlix.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResult<UserInfoDto>>  GetUserByIdAsync(string userId, CancellationToken ct = default);
}