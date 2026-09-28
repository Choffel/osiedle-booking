using Microsoft.AspNetCore.Identity;
using Users.Infrastructure.Identity;

namespace Users.Infrastructure.Data.InitialData;

public class RoleInitData
{
    public static async Task InitializeAsync(RoleManager<RoleIdentity> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))  // TODO !!!
        {
            var adminRole = new RoleIdentity{ Name = "Admin", NormalizedName = "ADMIN" };
            await roleManager.CreateAsync(adminRole);
        }
        
        if (!await roleManager.RoleExistsAsync("User"))    // TODO !!!
        {
            var userRole = new RoleIdentity{ Name = "User", NormalizedName = "USER" };
            await roleManager.CreateAsync(userRole);
        }
    }
}