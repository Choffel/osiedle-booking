using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Errors;
using Users.Domain.Entity.User;
using Users.Infrastructure.Identity.Models;

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

    public Task<bool> RoleExistsAsync(string roleName, CancellationToken ct = default)
    {
        return _roleManager.RoleExistsAsync(roleName);
    }

    /*public Task GetUserRolesAsync(UserId userId, CancellationChangeToken ct = default)
    {
        var user = _userManager.FindByIdAsync(userId.ToString());
        
        var userRoles = _userManager.GetRolesAsync(user.Result);

        return userRoles;
    }*/
    
    public async Task<Result> AddToRoleAsync(UserId userId, string roleName, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure(IdentityErrors.NotFound);

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            return Result.Failure(RoleErrors.AddUserToRoleFailed);

        return Result.Success();
    }
}