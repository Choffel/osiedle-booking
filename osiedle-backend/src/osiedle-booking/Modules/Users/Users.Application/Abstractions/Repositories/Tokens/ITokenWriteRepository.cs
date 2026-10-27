using Users.Domain.Entity.RefreshToken;

namespace Users.Application.Abstractions.Repositories.Tokens;

public interface ITokenWriteRepository
{
    Task<RefreshToken?> GetByTokenForUpdateAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
}