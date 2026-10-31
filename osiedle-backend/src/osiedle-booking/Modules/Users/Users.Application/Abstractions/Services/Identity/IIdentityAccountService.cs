using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs.Identity;

namespace Users.Application.Abstractions.Services.Identity;

public interface IIdentityAccountService
{
    Task<Result<UserId>> CreateIdentityAsync(string email, string userName, string password, CancellationToken ct = default);
    Task<Result<IdentityUserDto>> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<Result> UpdateUserNameAsync(UserId userId, string newName, CancellationToken ct = default);
}