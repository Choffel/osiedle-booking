using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Services.Identity;
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
    
    public async Task<Result<UserId>> CreateIdentityAsync(string email, string userName, string password,
        CancellationToken ct = default)
    {
        var identityUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName
        };

        var result = await _userManager.CreateAsync(identityUser, password);

        return result.Succeeded
            ? Result<UserId>.Success(UserId.From(identityUser.Id))
            : Result<UserId>.Failure(IdentityErrors.FromIdentityResult(string.Join(", ",
                result.Errors.Select(e => e.Description))));
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
    
    public async Task<Result> UpdateUserNameAsync(UserId userId, string newName, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<string>.Failure(IdentityErrors.NotFound);

        user.UserName = newName;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<string>.Failure(IdentityErrors.FromIdentityResult(string.Join(", ",
                result.Errors.Select(e => e.Description))));

        return Result<string>.Success(user.UserName);
    }
}