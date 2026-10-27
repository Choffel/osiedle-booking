using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User;

namespace Users.Application.Abstractions.Repositories.Users;

public interface IUserWriteRepository
{
    ValueTask<User?> GetByIdForUpdateAsync(UserId id, CancellationToken ct = default);
    void Add(User user);
}