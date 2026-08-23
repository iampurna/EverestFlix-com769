using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Ratings;
using EverestFlix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverestFlix.Api.Controllers;

[ApiController]
[Route("api/videos/{videoId:int}/rating")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService      _ratingService;
    private readonly ICurrentUserService _currentUser;

    public RatingsController(IRatingService ratingService, ICurrentUserService currentUser)
    {
        _ratingService = ratingService;
        _currentUser   = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<RatingSummaryDto>> GetSummary(int videoId, CancellationToken ct)
        => Ok(await _ratingService.GetSummaryAsync(videoId, _currentUser.UserId, ct));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Set(int videoId, [FromBody] SetRatingRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _ratingService.SetAsync(videoId, request.Value, userId, ct);
        if (result.Succeeded) return Ok(new { value = result.Value });

        return result.ErrorCode switch
        {
            ErrorCodes.NotFound => NotFound(new  { code = result.ErrorCode, errors = result.Errors }),
            _                   => BadRequest(new { code = result.ErrorCode, errors = result.Errors })
        };
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(int videoId, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _ratingService.DeleteAsync(videoId, userId, ct);
        if (result.Succeeded) return NoContent();

        return NotFound(new { code = result.ErrorCode, errors = result.Errors });
    }
}