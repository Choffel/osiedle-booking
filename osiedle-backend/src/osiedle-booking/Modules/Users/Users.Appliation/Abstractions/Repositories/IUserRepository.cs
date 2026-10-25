using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User;

namespace Users.Appliation.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    void Update(User user);
}