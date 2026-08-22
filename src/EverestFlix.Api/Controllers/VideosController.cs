using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Videos;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Constants;
using EverestFlix.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverestFlix.Api.Controllers;

[ApiController]
[Route("api/videos")]
public class VideosController : ControllerBase
{
    private readonly IVideoService       _videoService;
    private readonly ICurrentUserService _currentUser;

    public VideosController(IVideoService videoService, ICurrentUserService currentUser)
    {
        _videoService = videoService;
        _currentUser  = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<VideoSummaryDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
        => Ok(await _videoService.GetLatestAsync(page, pageSize, ct));

    [HttpGet("latest")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<VideoSummaryDto>>> GetLatest(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
        => Ok(await _videoService.GetLatestAsync(page, pageSize, ct));

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<VideoSummaryDto>>> Search(
        [FromQuery] VideoSearchQuery query, CancellationToken ct = default)
        => Ok(await _videoService.SearchAsync(query, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _videoService.GetByIdAsync(id, ct);
        if (!result.Succeeded)
            return NotFound(new { code = result.ErrorCode, errors = result.Errors });
        return Ok(result.Value);
    }

    /// <summary>
    /// Creator-only video upload. Adapts multipart IFormFile → transport-neutral VideoUpload
    /// at the API boundary so the Application layer stays independent of ASP.NET Core types.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Creator)]
    [RequestSizeLimit(100_000_000)]                 // 100 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
    public async Task<IActionResult> Create(
        [FromForm] string      title,
        [FromForm] string?     description,
        [FromForm] string      publisher,
        [FromForm] string      producer,
        [FromForm] string      genre,
        [FromForm] AgeRating   ageRating,
        IFormFile              videoFile,
        CancellationToken      ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (videoFile is null || videoFile.Length == 0)
            return BadRequest(new { code = ErrorCodes.ValidationError, errors = new[] { "VideoFile is required." } });

        await using var stream = videoFile.OpenReadStream();

        var request = new CreateVideoRequest
        {
            Title       = title,
            Description = description,
            Publisher   = publisher,
            Producer    = producer,
            Genre       = genre,
            AgeRating   = ageRating,
            VideoFile   = new VideoUpload
            {
                Content     = stream,
                FileName    = videoFile.FileName,
                ContentType = videoFile.ContentType,
                Length      = videoFile.Length
            }
        };

        var result = await _videoService.CreateAsync(request, userId, ct);
        if (!result.Succeeded)
            return BadRequest(new { code = result.ErrorCode, errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Creator)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVideoRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _videoService.UpdateAsync(id, request, userId, ct);
        if (result.Succeeded) return Ok(result.Value);

        return result.ErrorCode switch
        {
            ErrorCodes.NotFound  => NotFound(new  { code = result.ErrorCode, errors = result.Errors }),
            ErrorCodes.Forbidden => Forbid(),
            _                    => BadRequest(new { code = result.ErrorCode, errors = result.Errors })
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{Roles.Creator},{Roles.Admin}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId  = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var isAdmin = _currentUser.IsInRole(Roles.Admin);

        var result = await _videoService.DeleteAsync(id, userId, isAdmin, ct);
        if (result.Succeeded) return NoContent();

        return result.ErrorCode switch
        {
            ErrorCodes.NotFound  => NotFound(new  { code = result.ErrorCode, errors = result.Errors }),
            ErrorCodes.Forbidden => Forbid(),
            _                    => BadRequest(new { code = result.ErrorCode, errors = result.Errors })
        };
    }
}