using Users.Appliation.Abstractions;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public Task AddAsync(Domain.Entity.User.User user, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Domain.Entity.User.User?> GetByIdAsync(Domain.Entity.User.Value_objects.UserId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public void Update(Domain.Entity.User.User user)
    {
        throw new NotImplementedException();
    }
}