using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Repositories.Users;
using Users.Domain.Entity.User;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Users;

public class UserReadRepository : IUserReadRepository
{
    private readonly ApplicationDbContext _context;

    public UserReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        return _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}