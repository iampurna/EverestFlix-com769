using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Comments;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Entities;
using EverestFlix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Comments;

public class CommentService : ICommentService
{
    private const int MaxCommentLength = 1000;

    private readonly EverestFlixDbContext _db;
    private readonly ILogger<CommentService> _logger;


    public CommentService(
        EverestFlixDbContext db,
        ILogger<CommentService> logger)
    {
        _db = db;
        _logger = logger;
    }


    // -------------------------------------------------------------
    // Get comments for a video
    // -------------------------------------------------------------

    public async Task<PagedResponse<CommentDto>> GetForVideoAsync(
        int videoId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page =
            page < 1
                ? 1
                : page;

        pageSize =
            pageSize < 1
                ? 10
                : pageSize;

        pageSize =
            pageSize > 50
                ? 50
                : pageSize;


        var query =
            _db.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.VideoId == videoId)
                .OrderByDescending(c => c.CreatedAt);


        var total =
            await query.LongCountAsync(ct);


        var items =
            await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentDto
                {
                    Id =
                        c.Id,

                    VideoId =
                        c.VideoId,

                    UserId =
                        c.UserId,

                    UserName =
                        c.User != null
                            ? c.User.FullName
                            : string.Empty,

                    Text =
                        c.Text,

                    CreatedAt =
                        c.CreatedAt
                })
                .ToListAsync(ct);


        return new PagedResponse<CommentDto>
        {
            Items =
                items,

            Page =
                page,

            PageSize =
                pageSize,

            TotalItems =
                total
        };
    }


    // -------------------------------------------------------------
    // Add comment
    // -------------------------------------------------------------

    public async Task<Result<CommentDto>> AddAsync(
        int videoId,
        CreateCommentRequest request,
        string userId,
        CancellationToken ct = default)
    {
        // Never rely only on browser/controller validation.
        // The service validates the comment as well.
        if (request is null)
        {
            return Result<CommentDto>.Fail(
                ErrorCodes.ValidationError,
                "Comment is required.");
        }


        var text =
            request.Text?.Trim()
            ?? string.Empty;


        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<CommentDto>.Fail(
                ErrorCodes.ValidationError,
                "Comment cannot be empty.");
        }


        if (text.Length > MaxCommentLength)
        {
            return Result<CommentDto>.Fail(
                ErrorCodes.ValidationError,
                $"Comment must not exceed {MaxCommentLength} characters.");
        }


        var videoExists =
            await _db.Videos.AnyAsync(
                v =>
                    v.Id == videoId &&
                    v.IsPublished,
                ct);


        if (!videoExists)
        {
            return Result<CommentDto>.Fail(
                ErrorCodes.NotFound,
                "Video not found.");
        }


        var comment =
            new Comment
            {
                VideoId =
                    videoId,

                UserId =
                    userId,

                Text =
                    text,

                CreatedAt =
                    DateTime.UtcNow
            };


        _db.Comments.Add(
            comment);


        await _db.SaveChangesAsync(
            ct);


        var user =
            await _db.Users.FindAsync(
                new object?[]
                {
                    userId
                },
                ct);


        _logger.LogInformation(
            "Comment added: video={VideoId} user={UserId}",
            videoId,
            userId);


        return Result<CommentDto>.Success(
            new CommentDto
            {
                Id =
                    comment.Id,

                VideoId =
                    comment.VideoId,

                UserId =
                    comment.UserId,

                UserName =
                    user?.FullName
                    ?? string.Empty,

                Text =
                    comment.Text,

                CreatedAt =
                    comment.CreatedAt
            });
    }


    // -------------------------------------------------------------
    // Delete comment
    // -------------------------------------------------------------

    public async Task<Result<bool>> DeleteAsync(
        int commentId,
        string requestingUserId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var comment =
            await _db.Comments
                .Include(c => c.Video)
                .FirstOrDefaultAsync(
                    c => c.Id == commentId,
                    ct);


        if (comment is null)
        {
            return Result<bool>.Fail(
                ErrorCodes.NotFound,
                "Comment not found.");
        }


        var isAuthor =
            comment.UserId ==
            requestingUserId;


        var isVideoOwner =
            comment.Video?.CreatorId ==
            requestingUserId;


        if (!isAuthor &&
            !isAdmin &&
            !isVideoOwner)
        {
            return Result<bool>.Fail(
                ErrorCodes.Forbidden,
                "You cannot delete this comment.");
        }


        _db.Comments.Remove(
            comment);


        await _db.SaveChangesAsync(
            ct);


        _logger.LogInformation(
            "Comment deleted: {CommentId} by {UserId}",
            commentId,
            requestingUserId);


        return Result<bool>.Success(
            true);
    }
}