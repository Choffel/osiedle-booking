using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User;

namespace Users.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task Update(User user);
}