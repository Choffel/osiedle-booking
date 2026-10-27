using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs.Identity;
using Users.Domain.Errors;

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
            return Result<string>.Failure(UserErrors.NotFound);

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, password, lockoutOnFailure: true);

        if (result.Succeeded) return Result<string>.Success("Password is correct");
        if (result.IsLockedOut) return Result<string>.Failure(UserErrors.LockedOut);
        return Result<string>.Failure(UserErrors.InvalidCredentials);
    }
    
    public async Task<Result<string>> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<string>.Failure(UserErrors.NotFound);

        return Result<string>.Success(await _userManager.GeneratePasswordResetTokenAsync(user));
    }
    
    public async Task<Result<IdentityUserDto>> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<IdentityUserDto>.Failure(UserErrors.NotFound);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return Result<IdentityUserDto>.Failure(UserErrors.ResetPasswordFailed);

        return Result<IdentityUserDto>.Success(new IdentityUserDto(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed,
            Succeeded: true
        ));
    }
}