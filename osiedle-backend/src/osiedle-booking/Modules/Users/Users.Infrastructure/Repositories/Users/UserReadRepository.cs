using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.DTOs;
using Users.Domain.Entity.User;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Users;

public class UserReadRepository : IUserReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserDto?> GetProfileAsync(UserId id, CancellationToken ct = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Select(user => new UserDto(
                user.Id,
                 user.Name.FirstName,
                 user.Name.LastName,
                 user.Email
            ))
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
    
    public async Task<User> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), ct);
    }
}