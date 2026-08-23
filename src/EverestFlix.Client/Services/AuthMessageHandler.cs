using System.Net.Http.Headers;

namespace EverestFlix.Client.Services;

/// <summary>
/// Attaches the current JWT (if any) as a Bearer token on every outgoing request.
/// Registered as a DelegatingHandler in Program.cs so services don't have to know about auth.
/// </summary>
public class AuthMessageHandler : DelegatingHandler
{
    private readonly JwtAuthenticationStateProvider _authState;

    public AuthMessageHandler(JwtAuthenticationStateProvider authState)
    {
        _authState = authState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _authState.CurrentToken;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}