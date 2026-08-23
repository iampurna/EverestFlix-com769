using EverestFlix.Application.Common;
using EverestFlix.Application.Interfaces;
using EverestFlix.Domain.Constants;
using EverestFlix.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Admin;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminService>        _logger;

    public AdminService(UserManager<ApplicationUser> userManager, ILogger<AdminService> logger)
    {
        _userManager = userManager;
        _logger      = logger;
    }

    public async Task<Result<bool>> PromoteToCreatorAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<bool>.Fail(ErrorCodes.UserNotFound, "User not found.");

        if (await _userManager.IsInRoleAsync(user, Roles.Creator))
            return Result<bool>.Success(true);

        await _userManager.RemoveFromRoleAsync(user, Roles.Consumer);
        await _userManager.AddToRoleAsync(user, Roles.Creator);

        _logger.LogInformation("User promoted to Creator: {UserId} ({Email})", user.Id, user.Email);
        return Result<bool>.Success(true);
    }
}