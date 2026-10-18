namespace Users.Application.Abstractions.Services;

public interface IRoleService
{
    Task<bool> RoleExistsAsync(string roleName, CancellationToken ct);
}