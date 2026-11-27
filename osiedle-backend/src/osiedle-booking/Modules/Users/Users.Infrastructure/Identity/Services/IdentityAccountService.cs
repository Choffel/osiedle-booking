using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;
using Users.Application.Errors;
using Users.Infrastructure.Identity.Models;

namespace Users.Infrastructure.Identity.Services;

public class IdentityAccountService : IIdentityAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityAccountService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<Result<RegisterResponse>> CreateIdentityAsync(
        string email,
        string userName,
        CancellationToken ct = default)
    {
        var identityUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName
        };

        var result = await _userManager.CreateAsync(identityUser);

        if (!result.Succeeded)
        {
            var duplicateEmail = result.Errors.Any(error =>
                string.Equals(error.Code, "DuplicateEmail", StringComparison.OrdinalIgnoreCase));

            if (duplicateEmail)
                return Result<RegisterResponse>.Failure(IdentityErrors.DuplicateEmail);

            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Failed to create user.";
            return Result<RegisterResponse>.Failure(Error.Validation("Identity.CreateFailed", firstError));
        }

        var code = await _userManager.GenerateTwoFactorTokenAsync(
            identityUser,
            TokenOptions.DefaultEmailProvider);

        return Result<RegisterResponse>.Success(new RegisterResponse(UserId.From(identityUser.Id), code));
    }

    public async Task<Result> VerifyEmailConfirmationTokenAsync(string email, string code, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure(IdentityErrors.InvalidOtp);

        // Используем VerifyTwoFactorTokenAsync в пару к GenerateTwoFactorTokenAsync
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultEmailProvider,
            code.Trim());

        if (!isValid)
            return Result.Failure(IdentityErrors.InvalidOtp);

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
        }

        // Инвалидируем использованный код и сохраняем EmailConfirmed = true
        await _userManager.UpdateSecurityStampAsync(user);

        return Result.Success();
    }
    
    public async Task<Result<BookingOtpResult>> GetOrCreateAndGenerateOtpAsync(
        string email,
        CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim();
        var identityUser = await _userManager.FindByEmailAsync(normalizedEmail);
        var isNewUser = false;

        if (identityUser is null)
        {
            identityUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                UserName = normalizedEmail
            };

            var createResult = await _userManager.CreateAsync(identityUser);
            if (!createResult.Succeeded)
            {
                var firstError = createResult.Errors.FirstOrDefault()?.Description ?? "Failed to create identity user.";
                return Result<BookingOtpResult>.Failure(Error.Validation("Identity.CreateFailed", firstError));
            }

            isNewUser = true;
        }

        var code = await _userManager.GenerateTwoFactorTokenAsync(
            identityUser,
            TokenOptions.DefaultEmailProvider);

        return Result<BookingOtpResult>.Success(
            new BookingOtpResult(UserId.From(identityUser.Id), code, isNewUser));
    }
    
    public async Task<Result<IdentityUserDto>> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<IdentityUserDto>.Failure(IdentityErrors.NotFound);
        
        var userRoles = await _userManager.GetRolesAsync(user);

        return Result<IdentityUserDto>.Success(new IdentityUserDto(
            Id: user.Id,
            Email: user.Email,
            Roles: userRoles.AsReadOnly()
        ));
    }

    public async Task<Result<IdentityUserDto>> FindByUserIdAsync(UserId userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<IdentityUserDto>.Failure(IdentityErrors.NotFound);
        
        var userRoles = await _userManager.GetRolesAsync(user);

        return Result<IdentityUserDto>.Success(new IdentityUserDto(
            Id: user.Id,
            Email: user.Email,
            Roles: userRoles.AsReadOnly()
        ));
    }
    
    public async Task<Result> UpdateUserNameAsync(UserId userId, string newName, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);

        user.UserName = newName;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.FromIdentityResult(string.Join(", ",
                result.Errors.Select(e => e.Description))));

        return Result.Success();
    }
    
    public async Task<Result> UpdateUserEmailAsync(UserId userId, string newEmail, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);

        user.Email = newEmail;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.FromIdentityResult(string.Join(", ",
                result.Errors.Select(e => e.Description))));

        return Result.Success();
    }
}