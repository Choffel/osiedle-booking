using Microsoft.AspNetCore.Identity;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs.Identity;

namespace Users.Infrastructure.Identity;

public class RoleService : IRoleService
{
    private readonly RoleManager<RoleIdentity> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleService(RoleManager<RoleIdentity> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<bool> RoleExistsAsync(string roleName, CancellationToken ct)
    {
        var role = await _roleManager.RoleExistsAsync(roleName);
        return await _roleManager.RoleExistsAsync(roleName);
    }
    
    public async Task<Result<IdentityUserDto>> AddToRoleAsync(Guid userId, string roleName, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<IdentityUserDto>.Failure("User not found");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            return Result<IdentityUserDto>.Failure("Failed to add user to role");

        return Result<IdentityUserDto>.Success(new IdentityUserDto(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed,
            Succeeded: true
        ));
    }
}