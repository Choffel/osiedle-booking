using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs.Identity;
using Users.Application.Errors;
using Users.Infrastructure.Identity.Models;

namespace Users.Infrastructure.Identity.Services;

public class IdentityCredentialsService : IIdentityCredentialsService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityCredentialsService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    public async Task<Result> CheckPasswordAsync(
        UserId userId, string password, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, password, lockoutOnFailure: true);
        
        if (result.IsLockedOut)
            return Result.Failure(IdentityErrors.LockedOut);
        if (result.Succeeded)
            return Result.Success();
        
        return Result.Failure(IdentityErrors.InvalidCredentials);
    }
    
    public async Task<Result> GeneratePasswordResetTokenAsync(UserId userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);

        return Result.Success(await _userManager.GeneratePasswordResetTokenAsync(user));
    }
    
    public async Task<Result> ResetPasswordAsync(UserId userId, string token, string newPassword,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.FromIdentityResult(string.Join(", ",
                result.Errors.Select(e => e.Description))));
        
        var userRoles = await _userManager.GetRolesAsync(user);

        return Result.Success(new IdentityUserDto(
            Id: user.Id,
            Email: user.Email,
            Roles: userRoles.AsReadOnly()
        ));
    }
}