using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;

namespace Users.Application.Abstractions.Repositories.Tokens;

public interface ITokenWriteRepository
{
    Task<RefreshToken> GetByTokenForUpdateAsync(string token, CancellationToken ct = default);
    Task<RefreshToken?> GetByIdForUpdateAsync(RefreshTokenId id, CancellationToken ct = default);
    void Add(RefreshToken refreshToken, CancellationToken ct = default);
}