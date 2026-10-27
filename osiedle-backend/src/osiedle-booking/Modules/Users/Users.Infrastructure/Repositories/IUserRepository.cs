using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions.Repositories;
using Users.Infrastructure.Data;
using osiedle_booking.BuildingBlocks.ValueObjects;

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

    public async Task<Domain.Entity.User.User?> GetByIdAsync(UserId id, CancellationToken ct)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task Update(Domain.Entity.User.User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
}