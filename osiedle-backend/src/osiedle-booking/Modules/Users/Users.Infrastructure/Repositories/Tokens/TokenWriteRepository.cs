using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Domain.Entity.RefreshToken;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories.Tokens;

public class TokenWriteRepository : ITokenWriteRepository
{
    private readonly ApplicationDbContext _context;

    public TokenWriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public Task<RefreshToken?> GetByTokenForUpdateAsync(string token, CancellationToken ct = default)
    {
        return _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
    }
}