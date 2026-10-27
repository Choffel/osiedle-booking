using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Repositories.Users;
using Users.Domain.Entity.User;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Users;

public class UserWriteRepository : IUserWriteRepository
{
    private readonly ApplicationDbContext _context;

    public UserWriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public ValueTask<User?> GetByIdForUpdateAsync(UserId id, CancellationToken ct = default)
    {
        return _context.Users.FindAsync([id], ct);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }
}