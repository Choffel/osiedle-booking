using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs.Identity;

namespace Users.Application.Abstractions.Services;

public interface IRoleService
{
    Task<bool> RoleExistsAsync(string roleName, CancellationToken ct);
    Task<Result<IdentityUserDto>> AddToRoleAsync(Guid userId, string roleName, CancellationToken ct);
}