using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Comments;

namespace EverestFlix.Application.Interfaces;

public interface ICommentService
{
    Task<PagedResponse<CommentDto>> GetForVideoAsync(int videoId, int page, int pageSize, CancellationToken ct = default);
    Task<Result<CommentDto>>        AddAsync(int videoId, CreateCommentRequest request, string userId, CancellationToken ct = default);
    Task<Result<bool>>              DeleteAsync(int commentId, string requestingUserId, bool isAdmin, CancellationToken ct = default);
}