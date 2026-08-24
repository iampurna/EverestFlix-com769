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
    private const long MaxVideoBytes =
        100_000_000;


    // Multipart requests contain some metadata/headers in
    // addition to the actual video file.
    private const long MaxRequestBytes =
        105_000_000;


    private readonly IVideoService _videoService;

    private readonly ICurrentUserService _currentUser;


    public VideosController(
        IVideoService videoService,
        ICurrentUserService currentUser)
    {
        _videoService =
            videoService;

        _currentUser =
            currentUser;
    }


    // -----------------------------------------------------------------
    // Public video endpoints
    // -----------------------------------------------------------------

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<VideoSummaryDto>>>
        GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            CancellationToken ct = default)
    {
        return Ok(
            await _videoService.GetLatestAsync(
                page,
                pageSize,
                ct));
    }


    [HttpGet("latest")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<VideoSummaryDto>>>
        GetLatest(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            CancellationToken ct = default)
    {
        return Ok(
            await _videoService.GetLatestAsync(
                page,
                pageSize,
                ct));
    }


    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<VideoSummaryDto>>>
        Search(
            [FromQuery] VideoSearchQuery query,
            CancellationToken ct = default)
    {
        return Ok(
            await _videoService.SearchAsync(
                query,
                ct));
    }


    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var result =
            await _videoService.GetByIdAsync(
                id,
                ct);


        if (!result.Succeeded)
        {
            return NotFound(
                new
                {
                    code =
                        result.ErrorCode,

                    errors =
                        result.Errors
                });
        }


        return Ok(
            result.Value);
    }
[HttpPost("{id:int}/view")]
[AllowAnonymous]
public async Task<IActionResult> RecordView(
    int id,
    CancellationToken ct)
{
    var result =
        await _videoService.RecordViewAsync(
            id,
            ct);

    if (!result.Succeeded)
    {
        return NotFound(
            new
            {
                code = result.ErrorCode,
                errors = result.Errors
            });
    }

    return Ok(
        new
        {
            viewCount = result.Value
        });
}

    // -----------------------------------------------------------------
    // Creator upload
    // -----------------------------------------------------------------

    [HttpPost]
    [Authorize(Roles = Roles.Creator)]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Create(
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string publisher,
        [FromForm] string producer,
        [FromForm] string genre,
        [FromForm] AgeRating ageRating,
        IFormFile? videoFile,
        CancellationToken ct)
    {
        var userId =
            _currentUser.UserId;


        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }


        var errors =
            ValidateCreateRequest(
                title,
                description,
                publisher,
                producer,
                genre,
                ageRating,
                videoFile);


        if (errors.Count > 0)
        {
            return BadRequest(
                new
                {
                    code =
                        ErrorCodes.ValidationError,

                    errors =
                        errors.ToArray()
                });
        }


        // Validation above guarantees this is not null.
        var file =
            videoFile!;


        await using var stream =
            file.OpenReadStream();


        var request =
            new CreateVideoRequest
            {
                Title =
                    title.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(description)
                        ? null
                        : description.Trim(),

                Publisher =
                    publisher.Trim(),

                Producer =
                    producer.Trim(),

                Genre =
                    genre.Trim(),

                AgeRating =
                    ageRating,

                VideoFile =
                    new VideoUpload
                    {
                        Content =
                            stream,

                        FileName =
                            file.FileName,

                        ContentType =
                            file.ContentType,

                        Length =
                            file.Length
                    }
            };


        var result =
            await _videoService.CreateAsync(
                request,
                userId,
                ct);


        if (!result.Succeeded)
        {
            return BadRequest(
                new
                {
                    code =
                        result.ErrorCode,

                    errors =
                        result.Errors
                });
        }


        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id =
                    result.Value!.Id
            },
            result.Value);
    }


    // -----------------------------------------------------------------
    // Creator update
    // -----------------------------------------------------------------

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Creator)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateVideoRequest request,
        CancellationToken ct)
    {
        var userId =
            _currentUser.UserId;


        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }


        if (!Enum.IsDefined(
                typeof(AgeRating),
                request.AgeRating))
        {
            return BadRequest(
                new
                {
                    code =
                        ErrorCodes.ValidationError,

                    errors =
                        new[]
                        {
                            "Invalid age rating."
                        }
                });
        }


        if (string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Publisher) ||
            string.IsNullOrWhiteSpace(request.Producer) ||
            string.IsNullOrWhiteSpace(request.Genre))
        {
            return BadRequest(
                new
                {
                    code =
                        ErrorCodes.ValidationError,

                    errors =
                        new[]
                        {
                            "Title, Publisher, Producer and Genre are required."
                        }
                });
        }


        request.Title =
            request.Title.Trim();

        request.Description =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        request.Publisher =
            request.Publisher.Trim();

        request.Producer =
            request.Producer.Trim();

        request.Genre =
            request.Genre.Trim();


        var result =
            await _videoService.UpdateAsync(
                id,
                request,
                userId,
                ct);


        if (result.Succeeded)
        {
            return Ok(
                result.Value);
        }


        return result.ErrorCode switch
        {
            ErrorCodes.NotFound =>
                NotFound(
                    new
                    {
                        code =
                            result.ErrorCode,

                        errors =
                            result.Errors
                    }),

            ErrorCodes.Forbidden =>
                Forbid(),

            _ =>
                BadRequest(
                    new
                    {
                        code =
                            result.ErrorCode,

                        errors =
                            result.Errors
                    })
        };
    }


    // -----------------------------------------------------------------
    // Creator/Admin delete
    // -----------------------------------------------------------------

    [HttpDelete("{id:int}")]
    [Authorize(
        Roles = $"{Roles.Creator},{Roles.Admin}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var userId =
            _currentUser.UserId;


        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }


        var isAdmin =
            _currentUser.IsInRole(
                Roles.Admin);


        var result =
            await _videoService.DeleteAsync(
                id,
                userId,
                isAdmin,
                ct);


        if (result.Succeeded)
        {
            return NoContent();
        }


        return result.ErrorCode switch
        {
            ErrorCodes.NotFound =>
                NotFound(
                    new
                    {
                        code =
                            result.ErrorCode,

                        errors =
                            result.Errors
                    }),

            ErrorCodes.Forbidden =>
                Forbid(),

            _ =>
                BadRequest(
                    new
                    {
                        code =
                            result.ErrorCode,

                        errors =
                            result.Errors
                    })
        };
    }


    // -----------------------------------------------------------------
    // Validation helpers
    // -----------------------------------------------------------------

    private static List<string> ValidateCreateRequest(
        string? title,
        string? description,
        string? publisher,
        string? producer,
        string? genre,
        AgeRating ageRating,
        IFormFile? videoFile)
    {
        var errors =
            new List<string>();


        ValidateRequiredText(
            errors,
            title,
            "Title",
            200);


        ValidateRequiredText(
            errors,
            publisher,
            "Publisher",
            150);


        ValidateRequiredText(
            errors,
            producer,
            "Producer",
            150);


        ValidateRequiredText(
            errors,
            genre,
            "Genre",
            80);


        if (!string.IsNullOrEmpty(description) &&
            description.Length > 2000)
        {
            errors.Add(
                "Description must not exceed 2000 characters.");
        }


        if (!Enum.IsDefined(
                typeof(AgeRating),
                ageRating))
        {
            errors.Add(
                "Invalid age rating.");
        }


        if (videoFile is null)
        {
            errors.Add(
                "Video file is required.");

            return errors;
        }


        if (videoFile.Length <= 0)
        {
            errors.Add(
                "Video file is empty.");
        }


        if (videoFile.Length > MaxVideoBytes)
        {
            errors.Add(
                "Video must not exceed 100 MB.");
        }


        var extension =
            Path.GetExtension(
                videoFile.FileName);


        if (!string.Equals(
                extension,
                ".mp4",
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                "Only MP4 video files are supported.");
        }


        var mediaType =
            videoFile.ContentType?
                .Split(';', 2)[0]
                .Trim();


        if (!string.Equals(
                mediaType,
                "video/mp4",
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                "Video Content-Type must be video/mp4.");
        }


        return errors;
    }


    private static void ValidateRequiredText(
        ICollection<string> errors,
        string? value,
        string fieldName,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(
                $"{fieldName} is required.");

            return;
        }


        if (value.Trim().Length > maxLength)
        {
            errors.Add(
                $"{fieldName} must not exceed {maxLength} characters.");
        }
    }
}