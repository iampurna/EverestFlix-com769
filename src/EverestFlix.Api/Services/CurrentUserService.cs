using System.Security.Claims;
using EverestFlix.Application.Interfaces;

namespace EverestFlix.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email  => User?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList().AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}