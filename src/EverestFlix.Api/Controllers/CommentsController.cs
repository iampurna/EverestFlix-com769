using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Comments;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverestFlix.Api.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService     _commentService;
    private readonly ICurrentUserService _currentUser;

    public CommentsController(ICommentService commentService, ICurrentUserService currentUser)
    {
        _commentService = commentService;
        _currentUser    = currentUser;
    }

    [HttpGet("api/videos/{videoId:int}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<CommentDto>>> GetForVideo(
        int videoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await _commentService.GetForVideoAsync(videoId, page, pageSize, ct));

    [HttpPost("api/videos/{videoId:int}/comments")]
    [Authorize]
    public async Task<IActionResult> Add(int videoId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _commentService.AddAsync(videoId, request, userId, ct);
        if (result.Succeeded) return Created($"/api/comments/{result.Value!.Id}", result.Value);

        return result.ErrorCode switch
        {
            ErrorCodes.NotFound => NotFound(new  { code = result.ErrorCode, errors = result.Errors }),
            _                   => BadRequest(new { code = result.ErrorCode, errors = result.Errors })
        };
    }

    [HttpDelete("api/comments/{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var isAdmin = _currentUser.IsInRole(Roles.Admin);

        var result = await _commentService.DeleteAsync(id, userId, isAdmin, ct);
        if (result.Succeeded) return NoContent();

        return result.ErrorCode switch
        {
            ErrorCodes.NotFound  => NotFound(new  { code = result.ErrorCode, errors = result.Errors }),
            ErrorCodes.Forbidden => Forbid(),
            _                    => BadRequest(new { code = result.ErrorCode, errors = result.Errors })
        };
    }
}