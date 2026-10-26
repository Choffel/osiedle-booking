using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;

namespace Users.Infrastructure.Identity;

public class IdentityEmailService : IIdentityEmailService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityEmailService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<string>.Failure("User not found");
        
        return Result<string>.Success(await _userManager.GenerateEmailConfirmationTokenAsync(user));
    }
    
    public async Task<Result<string>> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<string>.Failure("User not found");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
            return Result<string>.Success("Email confirmed");
        
        return Result<string>.Failure("Failed to confirm email");
    }

}