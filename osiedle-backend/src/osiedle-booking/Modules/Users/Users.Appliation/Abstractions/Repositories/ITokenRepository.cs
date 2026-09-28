using Users.Domain.Entity.RefreshToken;

namespace Users.Appliation.Abstractions;

public interface ITokenRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<RefreshToken?> GetRefreshTokenByIdAsync(Guid tokenId, CancellationToken ct = default);
    Task DeleteRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    Task<IReadOnlyCollection<RefreshToken>> GetActiveRefreshTokensAsync(Guid userId, CancellationToken ct = default);
}