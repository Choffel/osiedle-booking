using Microsoft.Extensions.Options;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Abstractions.Services.Tokens;
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
    private readonly IIdentityAccountService _identityAccountService;
    private readonly IUserReadRepository _userReadRepository;
    
    public TokenService(IJwtTokenGenerator jwtTokenGenerator, ITokenWriteRepository tokenWriteRepository,
        IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtOptions, IIdentityAccountService identityAccountService,
        IUserReadRepository userReadRepository)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _tokenWriteRepository = tokenWriteRepository;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
        _identityAccountService = identityAccountService;
        _userReadRepository = userReadRepository;
    }

    public async Task<Result<TokenResponseDto>> GenerateTokensAsync(UserId userId, string email, IReadOnlyCollection<string> roles,
        CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(email, ct);
        
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(UserId.From(user.Value.Id), user.Value.Email, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        
        var refreshTokenEntity = RefreshToken.Create(
            RefreshTokenId.New(),
            refreshToken,
            UserId.From(user.Value.Id),
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            DateTime.UtcNow
        );
        
        _tokenWriteRepository.Add(refreshTokenEntity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(new TokenResponseDto(accessToken, refreshToken));
    }

    public async Task<Result<TokenResponseDto>> GenerateTokensByRefreshTokenAsync(string currentRefreshToken, CancellationToken ct = default)
    {
        var user = await _userReadRepository.GetUserByRefreshTokenAsync(currentRefreshToken, ct);
        
        var userRoles = await _identityAccountService.FindByEmailAsync(user.Email);
        
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, userRoles.Value.Email, userRoles.Value.Roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        
        var refreshTokenEntity = RefreshToken.Create(
            RefreshTokenId.New(),
            refreshToken,
            user.Id, 
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            DateTime.UtcNow
        );
        
        _tokenWriteRepository.Add(refreshTokenEntity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(new TokenResponseDto(accessToken, refreshToken));
    }
}