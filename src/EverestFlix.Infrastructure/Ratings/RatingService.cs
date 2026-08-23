using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Ratings;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Entities;
using EverestFlix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Ratings;

public class RatingService : IRatingService
{
    private readonly EverestFlixDbContext   _db;
    private readonly ILogger<RatingService> _logger;

    public RatingService(EverestFlixDbContext db, ILogger<RatingService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<RatingSummaryDto> GetSummaryAsync(int videoId, string? currentUserId, CancellationToken ct = default)
    {
        var ratings = _db.Ratings.AsNoTracking().Where(r => r.VideoId == videoId);

        var count    = await ratings.CountAsync(ct);
        var average  = count > 0 ? await ratings.AverageAsync(r => (double)r.Value, ct) : 0.0;
        var myRating = currentUserId is null
            ? (int?)null
            : await ratings.Where(r => r.UserId == currentUserId)
                           .Select(r => (int?)r.Value)
                           .FirstOrDefaultAsync(ct);

        return new RatingSummaryDto
        {
            Average  = Math.Round(average, 2),
            Count    = count,
            MyRating = myRating
        };
    }

    public async Task<Result<int>> SetAsync(int videoId, int value, string userId, CancellationToken ct = default)
    {
        if (value < 1 || value > 5)
            return Result<int>.Fail(ErrorCodes.ValidationError, "Rating must be between 1 and 5.");

        var videoExists = await _db.Videos.AnyAsync(v => v.Id == videoId && v.IsPublished, ct);
        if (!videoExists)
            return Result<int>.Fail(ErrorCodes.NotFound, "Video not found.");

        var existing = await _db.Ratings
            .FirstOrDefaultAsync(r => r.VideoId == videoId && r.UserId == userId, ct);

        if (existing is null)
        {
            _db.Ratings.Add(new Rating
            {
                VideoId   = videoId,
                UserId    = userId,
                Value     = value,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Value     = value;
            existing.CreatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Rating set: video={VideoId} user={UserId} value={Value}", videoId, userId, value);
        return Result<int>.Success(value);
    }

    public async Task<Result<bool>> DeleteAsync(int videoId, string userId, CancellationToken ct = default)
    {
        var existing = await _db.Ratings
            .FirstOrDefaultAsync(r => r.VideoId == videoId && r.UserId == userId, ct);

        if (existing is null)
            return Result<bool>.Fail(ErrorCodes.NotFound, "You have not rated this video.");

        _db.Ratings.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}