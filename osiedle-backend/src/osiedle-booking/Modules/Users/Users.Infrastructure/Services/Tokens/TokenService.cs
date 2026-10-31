using Microsoft.Extensions.Options;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Infrastructure.Options;

namespace Users.Infrastructure.Services.Tokens;

internal sealed class TokenService : ITokenService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITokenWriteRepository _tokenWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    
    public TokenService(IJwtTokenGenerator jwtTokenGenerator, ITokenWriteRepository tokenWriteRepository,
        IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtOptions)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _tokenWriteRepository = tokenWriteRepository;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Result<TokenResponseDto>> GenerateTokensAsync(UserId userId, string email,
        IReadOnlyCollection<string> roles, CancellationToken ct = default)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(userId, email, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        
        var refreshTokenEntity = RefreshToken.Create(
            RefreshTokenId.New(),
            refreshToken,
            userId, 
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            DateTime.UtcNow
        );
        
        _tokenWriteRepository.Add(refreshTokenEntity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(new TokenResponseDto(accessToken, refreshToken));
    }
}