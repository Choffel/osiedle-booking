using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs.Identity;

namespace Users.Infrastructure.Identity;

public class IdentityCredentialsService : IIdentityCredentialsService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityCredentialsService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    public async Task<Result<string>> CheckPasswordAsync(
        Guid userId, string password, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<string>.Failure("User not found");

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, password, lockoutOnFailure: true);

        if (result.Succeeded) return Result<string>.Success("Password is correct");
        if (result.IsLockedOut) return Result<string>.Failure("User is locked out");
        return Result<string>.Failure("Password is incorrect");
    }
    
    public async Task<Result<string>> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<string>.Failure("User not found");

        return Result<string>.Success(await _userManager.GeneratePasswordResetTokenAsync(user));
    }
    
    public async Task<Result<IdentityUserDto>> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<IdentityUserDto>.Failure("User not found");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return Result<IdentityUserDto>.Failure("Failed to reset password");

        return Result<IdentityUserDto>.Success(new IdentityUserDto(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed,
            Succeeded: true
        ));
    }
}