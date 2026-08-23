using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace EverestFlix.Client.Services;

/// <summary>
/// Lightweight authentication state provider for Blazor WASM.
/// Persists the JWT in localStorage and parses claims (including roles) from it.
/// Works with the built-in <AuthorizeView> component.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "everestflix.token";

    private readonly LocalStorageService _storage;
    private          string?             _cachedToken;

    public JwtAuthenticationStateProvider(LocalStorageService storage)
    {
        _storage = storage;
    }

    public string? CurrentToken => _cachedToken;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _cachedToken ??= await _storage.GetAsync(TokenKey);

        if (string.IsNullOrWhiteSpace(_cachedToken))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var identity = BuildIdentityFromToken(_cachedToken);
        if (identity is null)
        {
            await LogOutAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task LogInAsync(string token)
    {
        _cachedToken = token;
        await _storage.SetAsync(TokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogOutAsync()
    {
        _cachedToken = null;
        await _storage.RemoveAsync(TokenKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static ClaimsIdentity? BuildIdentityFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return null;

            var jwt = handler.ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow) return null;

            return new ClaimsIdentity(jwt.Claims, authenticationType: "jwt",
                nameType: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                roleType: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        }
        catch
        {
            return null;
        }
    }
}