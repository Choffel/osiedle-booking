namespace Users.Appliation.Abstractions;

public interface IRoleService
{
    Task<bool> RoleExistsAsync(string roleName, CancellationToken ct);
}