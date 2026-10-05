using Microsoft.AspNetCore.Identity;
using Users.Application.Abstractions;

namespace Users.Infrastructure.Identity;

public class RoleService : IRoleService
{
    private readonly RoleManager<RoleIdentity> _roleManager;

    public RoleService(RoleManager<RoleIdentity> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<bool> RoleExistsAsync(string roleName, CancellationToken ct)
    {
        var role = await _roleManager.RoleExistsAsync(roleName);
        return await _roleManager.RoleExistsAsync(roleName);
    }
}