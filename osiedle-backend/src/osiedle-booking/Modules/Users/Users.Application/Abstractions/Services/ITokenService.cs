using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs;

namespace Users.Application.Abstractions;

public interface ITokenService
{
    Task<TokenResponseDto> GetTokens(UserDto userDto, CancellationToken ct);
    Task<Result<bool>> RevokeRefreshTokenAsync(CancellationToken cancellationToken = default);
    Task<Result<TokenResponseDto>> GetNewAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ActiveSessionDto>>> GetActiveSessionsAsync(CancellationToken ct = default);
    Task<Result<string>> RevokeSessionAsync(Guid tokenId, CancellationToken ct = default);
}