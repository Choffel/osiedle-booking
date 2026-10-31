using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;

namespace Users.Application.Services;

public class TokenService : ITokenService // TODO подумать над размещением данного сервиса. на уровне приложения или инфраструктуры
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITokenWriteRepository _tokenWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public TokenService(IJwtTokenGenerator jwtTokenGenerator, ITokenWriteRepository tokenWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _tokenWriteRepository = tokenWriteRepository;
        _unitOfWork = unitOfWork;
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
            DateTime.UtcNow.AddDays(30), // TODO: Make this configurable
            DateTime.UtcNow
        );
        
        _tokenWriteRepository.Add(refreshTokenEntity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(new TokenResponseDto(accessToken, refreshToken));
    }
}