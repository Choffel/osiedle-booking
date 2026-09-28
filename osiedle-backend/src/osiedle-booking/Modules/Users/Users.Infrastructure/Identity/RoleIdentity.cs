using Microsoft.AspNetCore.Identity;

namespace Users.Infrastructure.Identity;

public class RoleIdentity : IdentityRole<Guid>
{
    public ICollection<IdentityUserRole<Guid>>? UserRoles { get; set; }
}