using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Errors;
using Users.Infrastructure.Identity.Models;

namespace Users.Infrastructure.Identity.Services;

public class IdentityEmailService : IIdentityEmailService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityEmailService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<Result> GenerateEmailConfirmationTokenAsync(UserId userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);
        
        return Result.Success(await _userManager.GenerateEmailConfirmationTokenAsync(user));
    }
    
    public async Task<Result> ConfirmEmailAsync(UserId userId, string token, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
            return Result.Success();
        
        return Result.Failure(IdentityErrors.EmailConfirmationFailed);
    }
}