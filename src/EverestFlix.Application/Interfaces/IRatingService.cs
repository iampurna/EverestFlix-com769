using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Ratings;

namespace EverestFlix.Application.Interfaces;

public interface IRatingService
{
    Task<RatingSummaryDto>  GetSummaryAsync(int videoId, string? currentUserId, CancellationToken ct = default);
    Task<Result<int>>       SetAsync(int videoId, int value, string userId, CancellationToken ct = default);
    Task<Result<bool>>      DeleteAsync(int videoId, string userId, CancellationToken ct = default);
}