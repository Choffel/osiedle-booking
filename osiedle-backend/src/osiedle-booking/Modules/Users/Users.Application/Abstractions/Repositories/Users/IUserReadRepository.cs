using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs;
using Users.Domain.Entity.User;

namespace Users.Application.Abstractions.Repositories.Users;

public interface IUserReadRepository
{
    Task<UserDto?> GetProfileAsync(UserId id, CancellationToken ct = default);
    Task<User> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}