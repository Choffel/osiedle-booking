using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;

namespace Users.Application.Abstractions.Repositories.Tokens;

public interface ITokenReadRepository
{ 
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task<RefreshToken?> GetByIdAsync(RefreshTokenId id, CancellationToken ct = default); 
        Task<IReadOnlyCollection<RefreshToken>> GetActiveTokensByUserIdAsync(UserId userId, CancellationToken ct = default);
}