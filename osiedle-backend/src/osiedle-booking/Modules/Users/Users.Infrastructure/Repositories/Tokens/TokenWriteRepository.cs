using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Tokens;

public class TokenWriteRepository : ITokenWriteRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TokenWriteRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task<RefreshToken?> GetByTokenForUpdateAsync(string token, CancellationToken ct = default)
    {
        return _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public Task<RefreshToken?> GetByIdForUpdateAsync(RefreshTokenId id, CancellationToken ct = default)
    {
        return _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public void Add(RefreshToken refreshToken, CancellationToken ct = default)
    { 
        _dbContext.RefreshTokens.Add(refreshToken);
    }
}