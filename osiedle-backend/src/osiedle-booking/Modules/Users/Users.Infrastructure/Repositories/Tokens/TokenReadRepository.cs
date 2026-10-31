using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Tokens;

public class TokenReadRepository : ITokenReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TokenReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }
    
    public Task<RefreshToken?> GetByIdAsync(RefreshTokenId id, CancellationToken ct = default)
    {
        return _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
    
    public async Task<IReadOnlyCollection<RefreshToken>> GetActiveTokensByUserIdAsync(
        UserId userId, 
        CancellationToken ct = default)
    {
        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.IsActive)
            .ToListAsync(ct);
    }
}