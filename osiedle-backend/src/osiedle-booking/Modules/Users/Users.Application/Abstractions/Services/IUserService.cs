using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs;

namespace Users.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result> UpdateUserNameAsync(UserId userId, string email, UpdateUserDto userDto, CancellationToken ct = default);
    Task<Result> UpdateUserEmailAsyc(UserId userId, string email, CancellationToken ct = default);
    Task<Result<CurrentUserDto>> GetUserByIdAsync(UserId userId, CancellationToken ct = default);
}