using Users.Application.Abstractions.Repositories;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddAsync(Domain.Entity.User.User user, CancellationToken ct)
    {
        await _dbContext.Users.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct); 
    }

    public Task<Domain.Entity.User.User?> GetByIdAsync(Domain.Entity.User.Value_objects.UserId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task Update(Domain.Entity.User.User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
}