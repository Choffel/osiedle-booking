using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Tokens;

public class TokenReadRepository : ITokenReadRepository
{
    private readonly ApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public TokenReadRepository(ApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }
    
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }
    
    public Task<RefreshToken?> GetByIdAsync(RefreshTokenId id, CancellationToken ct = default)
    {
        return _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
    
    public async Task<IReadOnlyCollection<RefreshToken>> GetActiveTokensByUserIdAsync(
        UserId userId, 
        CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);
    }
}