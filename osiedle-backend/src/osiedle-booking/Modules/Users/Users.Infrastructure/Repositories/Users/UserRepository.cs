using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Repositories.Users;
using Users.Domain.Entity.User;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public void Add(User user)
    {
        _dbContext.Users.Add(user);
    }
}