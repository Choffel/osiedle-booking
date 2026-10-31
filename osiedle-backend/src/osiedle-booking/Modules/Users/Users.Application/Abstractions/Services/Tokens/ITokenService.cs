using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs;

namespace Users.Application.Abstractions.Services;

public interface ITokenService
{
    Task<Result<TokenResponseDto>> GenerateTokensAsync(UserId userId, string email, IReadOnlyCollection<string> roles,
        CancellationToken ct = default);
    //Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    //Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    
    /*Task<TokenResponseDto> GetTokens(UserDto userDto, CancellationToken ct);
    Task<Result<bool>> RevokeRefreshTokenAsync(CancellationToken cancellationToken = default);
    Task<Result<TokenResponseDto>> GetNewAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ActiveSessionDto>>> GetActiveSessionsAsync(CancellationToken ct = default);
    Task<Result<string>> RevokeSessionAsync(Guid tokenId, CancellationToken ct = default);*/
}