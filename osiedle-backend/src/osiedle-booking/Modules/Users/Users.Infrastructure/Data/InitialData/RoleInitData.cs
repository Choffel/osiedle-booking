using Microsoft.AspNetCore.Identity;
using Users.Infrastructure.Identity.Models;

namespace Users.Infrastructure.Data.InitialData;

public static class RoleInitData
{
    public static async Task InitializeAsync(IReadOnlyCollection<string> roles, RoleManager<RoleIdentity> roleManager)
    {
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleIdentity = new RoleIdentity(role);
                var result = await roleManager.CreateAsync(roleIdentity);
                
                if(!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create role:" + 
                                                        $"{string.Join(", ",
                                                            result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}