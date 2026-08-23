using EverestFlix.Application.Common;

namespace EverestFlix.Application.Interfaces;

public interface IAdminService
{
    Task<Result<bool>> PromoteToCreatorAsync(string userId, CancellationToken ct = default);
}