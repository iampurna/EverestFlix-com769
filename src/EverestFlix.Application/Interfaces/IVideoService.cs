using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Videos;

namespace EverestFlix.Application.Interfaces;

public interface IVideoService
{
    Task<PagedResponse<VideoSummaryDto>> GetLatestAsync(
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<PagedResponse<VideoSummaryDto>> SearchAsync(
        VideoSearchQuery query,
        CancellationToken ct = default);

    Task<Result<VideoDetailDto>> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task<Result<long>> RecordViewAsync(
        int id,
        CancellationToken ct = default);

    Task<Result<VideoDetailDto>> CreateAsync(
        CreateVideoRequest request,
        string creatorId,
        CancellationToken ct = default);

    Task<Result<VideoDetailDto>> UpdateAsync(
        int id,
        UpdateVideoRequest request,
        string requestingUserId,
        CancellationToken ct = default);

    Task<Result<bool>> DeleteAsync(
        int id,
        string requestingUserId,
        bool isAdmin,
        CancellationToken ct = default);

    Task<PagedResponse<VideoSummaryDto>> GetCreatorVideosAsync(
        string creatorId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Result<CreatorDashboardDto>> GetCreatorDashboardAsync(
        string creatorId,
        CancellationToken ct = default);
}