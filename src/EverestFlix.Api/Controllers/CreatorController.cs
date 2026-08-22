using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Videos;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverestFlix.Api.Controllers;

[ApiController]
[Route("api/creator")]
[Authorize(Roles = Roles.Creator)]
public class CreatorController : ControllerBase
{
    private readonly IVideoService       _videoService;
    private readonly ICurrentUserService _currentUser;

    public CreatorController(IVideoService videoService, ICurrentUserService currentUser)
    {
        _videoService = videoService;
        _currentUser  = currentUser;
    }

    [HttpGet("videos")]
    public async Task<ActionResult<PagedResponse<VideoSummaryDto>>> MyVideos(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        return Ok(await _videoService.GetCreatorVideosAsync(userId, page, pageSize, ct));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _videoService.GetCreatorDashboardAsync(userId, ct);
        if (!result.Succeeded)
            return BadRequest(new { code = result.ErrorCode, errors = result.Errors });
        return Ok(result.Value);
    }
}