using EverestFlix.Application.DTOs.Auth;
using EverestFlix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverestFlix.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService        _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        if (result.Succeeded) return Ok(result.Value);

        return result.ErrorCode switch
        {
            "EMAIL_TAKEN" => Conflict(new   { code = result.ErrorCode, errors = result.Errors }),
            _             => BadRequest(new { code = result.ErrorCode, errors = result.Errors })
        };
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (result.Succeeded) return Ok(result.Value);

        return Unauthorized(new { code = result.ErrorCode, errors = result.Errors });
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _authService.GetUserByIdAsync(userId, ct);
        if (!result.Succeeded) return NotFound(new { code = result.ErrorCode, errors = result.Errors });

        return Ok(result.Value);
    }
}