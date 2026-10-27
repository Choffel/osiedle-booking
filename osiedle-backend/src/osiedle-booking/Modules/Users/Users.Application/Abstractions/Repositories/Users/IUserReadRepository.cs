using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User;

namespace Users.Application.Abstractions.Repositories.Users;

public interface IUserReadRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
}