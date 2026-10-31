using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs;

namespace Users.Application.Abstractions.Repositories.Users;

public interface IUserReadRepository
{
    Task<UserDto?> GetProfileAsync(UserId id, CancellationToken ct = default);
}