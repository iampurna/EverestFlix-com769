using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Auth;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Constants;
using EverestFlix.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EverestFlix.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService             _jwtTokenService;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager     = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return AuthResult<AuthResponse>.Fail("EMAIL_TAKEN", "An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName       = request.Email,
            Email          = request.Email,
            EmailConfirmed = true,   // No email-confirmation flow in coursework scope
            FullName       = request.FullName,
            CreatedAt      = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AuthResult<AuthResponse>.Fail(
                "VALIDATION_ERROR",
                createResult.Errors.Select(e => e.Description).ToArray());
        }

        // CRITICAL: every public self-registration is a Consumer. No exceptions.
        // Creator role is granted only via seeding or (future) admin-only endpoint.
        await _userManager.AddToRoleAsync(user, Roles.Consumer);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.CreateToken(user, roles);

        return AuthResult<AuthResponse>.Success(new AuthResponse
        {
            Token     = token,
            ExpiresAt = expiresAt,
            User      = MapToDto(user, roles)
        });
    }

    public async Task<AuthResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return AuthResult<AuthResponse>.Fail("INVALID_CREDENTIALS", "Invalid email or password.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return AuthResult<AuthResponse>.Fail("INVALID_CREDENTIALS", "Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.CreateToken(user, roles);

        return AuthResult<AuthResponse>.Success(new AuthResponse
        {
            Token     = token,
            ExpiresAt = expiresAt,
            User      = MapToDto(user, roles)
        });
    }

    public async Task<AuthResult<UserInfoDto>> GetUserByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return AuthResult<UserInfoDto>.Fail("USER_NOT_FOUND", "User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return AuthResult<UserInfoDto>.Success(MapToDto(user, roles));
    }

    private static UserInfoDto MapToDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id       = user.Id,
        Email    = user.Email ?? string.Empty,
        FullName = user.FullName,
        Roles    = roles.ToList().AsReadOnly()
    };
}