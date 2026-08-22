using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Videos;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Constants;
using EverestFlix.Domain.Entities;
using EverestFlix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Videos;

public class VideoService : IVideoService
{
    private readonly EverestFlixDbContext  _db;
    private readonly IVideoStorageService  _storage;
    private readonly ILogger<VideoService> _logger;

    public VideoService(
        EverestFlixDbContext db,
        IVideoStorageService storage,
        ILogger<VideoService> logger)
    {
        _db      = db;
        _storage = storage;
        _logger  = logger;
    }

    public async Task<PagedResponse<VideoSummaryDto>> GetLatestAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Normalize(page, pageSize);

        var query = _db.Videos
            .AsNoTracking()
            .Where(v => v.IsPublished)
            .OrderByDescending(v => v.CreatedAt);

        var total = await query.LongCountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapSummary(v))
            .ToListAsync(ct);

        return new PagedResponse<VideoSummaryDto>
        {
            Items      = items,
            Page       = page,
            PageSize   = pageSize,
            TotalItems = total
        };
    }

    public async Task<PagedResponse<VideoSummaryDto>> SearchAsync(VideoSearchQuery query, CancellationToken ct = default)
    {
        var (page, pageSize) = Normalize(query.Page, query.PageSize);

        var q = _db.Videos.AsNoTracking().Where(v => v.IsPublished);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLower();
            q = q.Where(v =>
                v.Title.ToLower().Contains(term) ||
                v.Publisher.ToLower().Contains(term) ||
                v.Producer.ToLower().Contains(term) ||
                v.Genre.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Genre))
        {
            var genre = query.Genre.Trim().ToLower();
            q = q.Where(v => v.Genre.ToLower() == genre);
        }

        var total = await q.LongCountAsync(ct);

        var items = await q
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapSummary(v))
            .ToListAsync(ct);

        return new PagedResponse<VideoSummaryDto>
        {
            Items      = items,
            Page       = page,
            PageSize   = pageSize,
            TotalItems = total
        };
    }

    public async Task<Result<VideoDetailDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Increment view count atomically (one SQL UPDATE, no read-modify-write)
        await _db.Videos
            .Where(v => v.Id == id && v.IsPublished)
            .ExecuteUpdateAsync(setters => setters.SetProperty(v => v.ViewCount, v => v.ViewCount + 1), ct);

        var video = await _db.Videos
            .AsNoTracking()
            .Include(v => v.Creator)
            .FirstOrDefaultAsync(v => v.Id == id && v.IsPublished, ct);

        if (video is null)
            return Result<VideoDetailDto>.Fail(ErrorCodes.NotFound, "Video not found.");

        return Result<VideoDetailDto>.Success(MapDetail(video));
    }

    public async Task<Result<VideoDetailDto>> CreateAsync(CreateVideoRequest request, string creatorId, CancellationToken ct = default)
    {
        string videoUrl;
        try
        {
            videoUrl = await _storage.SaveAsync(request.VideoFile, ct);
        }
        catch (InvalidOperationException ex)
        {
            return Result<VideoDetailDto>.Fail(ErrorCodes.StorageFailed, ex.Message);
        }

        var video = new Video
        {
            Title       = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Publisher   = request.Publisher.Trim(),
            Producer    = request.Producer.Trim(),
            Genre       = request.Genre.Trim(),
            AgeRating   = request.AgeRating,
            VideoUrl    = videoUrl,
            CreatorId   = creatorId,
            CreatedAt   = DateTime.UtcNow,
            IsPublished = true
        };

        _db.Videos.Add(video);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Video created: {VideoId} by {CreatorId}", video.Id, creatorId);

        // Reload with Creator navigation for the DTO
        var saved = await _db.Videos.AsNoTracking()
            .Include(v => v.Creator)
            .FirstAsync(v => v.Id == video.Id, ct);

        return Result<VideoDetailDto>.Success(MapDetail(saved));
    }

    public async Task<Result<VideoDetailDto>> UpdateAsync(int id, UpdateVideoRequest request, string requestingUserId, CancellationToken ct = default)
    {
        var video = await _db.Videos
            .Include(v => v.Creator)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (video is null)
            return Result<VideoDetailDto>.Fail(ErrorCodes.NotFound, "Video not found.");

        if (video.CreatorId != requestingUserId)
            return Result<VideoDetailDto>.Fail(ErrorCodes.Forbidden, "You do not own this video.");

        video.Title       = request.Title.Trim();
        video.Description = request.Description?.Trim();
        video.Publisher   = request.Publisher.Trim();
        video.Producer    = request.Producer.Trim();
        video.Genre       = request.Genre.Trim();
        video.AgeRating   = request.AgeRating;
        video.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Result<VideoDetailDto>.Success(MapDetail(video));
    }

    public async Task<Result<bool>> DeleteAsync(int id, string requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (video is null)
            return Result<bool>.Fail(ErrorCodes.NotFound, "Video not found.");

        if (!isAdmin && video.CreatorId != requestingUserId)
            return Result<bool>.Fail(ErrorCodes.Forbidden, "You do not own this video.");

        var url = video.VideoUrl;
        _db.Videos.Remove(video);
        await _db.SaveChangesAsync(ct);
        await _storage.DeleteAsync(url, ct);

        _logger.LogInformation("Video deleted: {VideoId} by {UserId}", id, requestingUserId);
        return Result<bool>.Success(true);
    }

    public async Task<PagedResponse<VideoSummaryDto>> GetCreatorVideosAsync(string creatorId, int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Normalize(page, pageSize);

        var query = _db.Videos
            .AsNoTracking()
            .Where(v => v.CreatorId == creatorId)
            .OrderByDescending(v => v.CreatedAt);

        var total = await query.LongCountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapSummary(v))
            .ToListAsync(ct);

        return new PagedResponse<VideoSummaryDto>
        {
            Items      = items,
            Page       = page,
            PageSize   = pageSize,
            TotalItems = total
        };
    }

    public async Task<Result<CreatorDashboardDto>> GetCreatorDashboardAsync(string creatorId, CancellationToken ct = default)
    {
        var totalVideos = await _db.Videos.CountAsync(v => v.CreatorId == creatorId, ct);
        var totalViews  = await _db.Videos.Where(v => v.CreatorId == creatorId)
                                          .SumAsync(v => (long?)v.ViewCount, ct) ?? 0;

        var ratings = _db.Ratings.Where(r => r.Video!.CreatorId == creatorId);
        var avgRating = await ratings.AnyAsync(ct)
            ? await ratings.AverageAsync(r => (double)r.Value, ct)
            : 0.0;

        var totalComments = await _db.Comments
            .CountAsync(c => c.Video!.CreatorId == creatorId, ct);

        return Result<CreatorDashboardDto>.Success(new CreatorDashboardDto
        {
            TotalVideos   = totalVideos,
            TotalViews    = totalViews,
            AverageRating = Math.Round(avgRating, 2),
            TotalComments = totalComments
        });
    }

    // ---------- helpers ----------

    private static (int page, int pageSize) Normalize(int page, int pageSize)
    {
        page     = page     < 1  ? 1  : page;
        pageSize = pageSize < 1  ? 12 : pageSize;
        pageSize = pageSize > 50 ? 50 : pageSize;
        return (page, pageSize);
    }

    private static VideoSummaryDto MapSummary(Video v) => new()
    {
        Id           = v.Id,
        Title        = v.Title,
        Publisher    = v.Publisher,
        Genre        = v.Genre,
        AgeRating    = v.AgeRating,
        VideoUrl     = v.VideoUrl,
        ThumbnailUrl = v.ThumbnailUrl,
        CreatorId    = v.CreatorId,
        CreatorName  = v.Creator != null ? v.Creator.FullName : string.Empty,
        CreatedAt    = v.CreatedAt,
        ViewCount    = v.ViewCount
    };

    private static VideoDetailDto MapDetail(Video v) => new()
    {
        Id           = v.Id,
        Title        = v.Title,
        Description  = v.Description,
        Publisher    = v.Publisher,
        Producer     = v.Producer,
        Genre        = v.Genre,
        AgeRating    = v.AgeRating,
        VideoUrl     = v.VideoUrl,
        ThumbnailUrl = v.ThumbnailUrl,
        CreatorId    = v.CreatorId,
        CreatorName  = v.Creator != null ? v.Creator.FullName : string.Empty,
        CreatedAt    = v.CreatedAt,
        UpdatedAt    = v.UpdatedAt,
        ViewCount    = v.ViewCount
    };
}