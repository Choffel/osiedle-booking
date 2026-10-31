using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;


namespace Users.Application.Abstractions.Services.Identity;

public interface IRoleService
{
    Task<bool> RoleExistsAsync(string roleName, CancellationToken ct);
    Task<Result> AddToRoleAsync(UserId userId, string roleName, CancellationToken ct);
}