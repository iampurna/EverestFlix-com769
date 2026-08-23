using EverestFlix.Application.Common;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverestFlix.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("users/{userId}/promote-to-creator")]
    public async Task<IActionResult> PromoteToCreator(string userId, CancellationToken ct)
    {
        var result = await _adminService.PromoteToCreatorAsync(userId, ct);
        if (result.Succeeded) return Ok(new { promoted = true, userId });

        return result.ErrorCode switch
        {
            ErrorCodes.UserNotFound => NotFound(new  { code = result.ErrorCode, errors = result.Errors }),
            _                       => BadRequest(new { code = result.ErrorCode, errors = result.Errors })
        };
    }
}