using Users.Domain.Entity.User;
using Users.Domain.Entity.User.Value_objects;

namespace Users.Appliation.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    void Update(User user);
}