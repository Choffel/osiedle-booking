using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs.Identity;

namespace Users.Application.Abstractions.Services;

public interface IIdentityAccountService
{
    Task<Result<Guid>> CreateIdentityAsync(string email, string userName, string password, CancellationToken ct);
    Task<Result<IdentityUserDto?>> FindByEmailAsync(string email, CancellationToken ct);
    Task<Result<string>> UpdateUserNameAsync(Guid userId, string newName, CancellationToken ct);
}