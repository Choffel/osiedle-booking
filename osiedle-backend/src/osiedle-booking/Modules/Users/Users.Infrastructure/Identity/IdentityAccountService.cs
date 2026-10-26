using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs.Identity;

namespace Users.Infrastructure.Identity;

public class IdentityAccountService : IIdentityAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityAccountService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<Result<Guid>> CreateIdentityAsync(string email, string userName, string password, CancellationToken ct)
    {
        var identityUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName
        };

        var result = await _userManager.CreateAsync(identityUser, password);

        return result.Succeeded
            ? Result<Guid>.Success(identityUser.Id)
            : Result<Guid>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
    
    public async Task<Result<IdentityUserDto?>> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result<IdentityUserDto?>.Failure("User not found");

        var response = new IdentityUserDto(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed,
            Succeeded: true
        );
        
        return Result<IdentityUserDto?>.Success(response);
    }
    
    public async Task<Result<string>> UpdateUserNameAsync(Guid userId ,string newName, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<string>.Failure("User not found");

        user.UserName = newName;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<string>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        return Result<string>.Success(user.UserName);
    }
}