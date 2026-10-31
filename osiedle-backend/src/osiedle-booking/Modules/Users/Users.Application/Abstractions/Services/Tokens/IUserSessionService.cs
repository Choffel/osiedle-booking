using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs;
using Users.Domain.Entity.RefreshToken.ValueObjects;

namespace Users.Application.Abstractions.Services.Tokens;

public interface IUserSessionService
{
    Task<Result> GetActiveSessionsAsync(UserId userId, string currentRefreshToken,
        CancellationToken ct = default);
    
    Task<Result> RevokeSessionAsync(UserId userId, RefreshTokenId tokenId, string currentRefreshToken,
        CancellationToken ct = default);
}