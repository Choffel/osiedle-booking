using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs;

namespace Users.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result> UpdateUserAsync(UserId userId, string email, UpdateUserDto userDto, CancellationToken ct = default);
    Task<Result> GetUserByIdAsync(UserId userId, CancellationToken ct = default);
}