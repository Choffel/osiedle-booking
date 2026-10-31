using Microsoft.AspNetCore.Identity;

namespace Users.Infrastructure.Identity.Models;

public class RoleIdentity : IdentityRole<Guid>
{
    public RoleIdentity() { }
    
    public RoleIdentity(string roleName) : base(roleName)
    {
    }
    
    public ICollection<IdentityUserRole<Guid>> UserRoles { get; } = new List<IdentityUserRole<Guid>>();
}