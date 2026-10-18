using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Appliation.Abstractions;
using Users.Appliation.DTOs;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Errors;
using Users.Infrastructure.Identity;

namespace Users.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ITokenRepository _tokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TokenService> _logger;
    private readonly IUserContext _userContext;


    public TokenService(IConfiguration configuration,
        ITokenRepository tokenRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<TokenService> logger,
        IUserContext userContext
        )
    {
        _configuration = configuration;
        _tokenRepository = tokenRepository;
        _userManager = userManager;
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<TokenResponseDto> GetTokens(UserDto user, CancellationToken cancellationToken = default)
    {
        var accessToken = await CreateTokenAsync(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);


        return new TokenResponseDto(accessToken, refreshToken);
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        await _tokenRepository.AddRefreshTokenAsync(refreshToken);

        return token;
    }

    public async Task<Result<TokenResponseDto>> GetNewAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var storedRefreshToken = await _tokenRepository.GetRefreshTokenAsync(_userContext.RefreshToken);
        if (storedRefreshToken == null || storedRefreshToken.IsExpired)
        {
            return Result<TokenResponseDto>.Failure(UserErrors.InvalidRefreshToken);
        }

        storedRefreshToken.ExpiresAt = DateTime.UtcNow;

        var newRefreshToken = await CreateRefreshTokenAsync(storedRefreshToken.UserId);

        var user = await _userManager.FindByIdAsync(storedRefreshToken.UserId.ToString());
        if (user == null)
        {
            return Result<TokenResponseDto>.Failure(UserErrors.NotFound);
        }

        var userDto = new UserDto(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty);

        var accessToken = await CreateTokenAsync(userDto);

        return Result<TokenResponseDto>.Success(new TokenResponseDto(accessToken, newRefreshToken));
    }

    public async Task<Result<bool>> RevokeRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await _tokenRepository.GetRefreshTokenAsync(_userContext.RefreshToken);
        if (refreshToken == null || refreshToken.UserId != _userContext.UserId || refreshToken.IsExpired)
        {
            return Result<bool>.Failure(UserErrors.InvalidRefreshToken);
        }

        refreshToken.ExpiresAt = DateTime.UtcNow;
        

        return Result<bool>.Success(true);
    }
    
    public async Task<Result<IEnumerable<ActiveSessionDto>>> GetActiveSessionsAsync(CancellationToken ct = default)
    {
        var refreshTokens = await _tokenRepository.GetActiveRefreshTokensAsync(_userContext.UserId, ct);

        var activeSessions = refreshTokens.Select(rt => new ActiveSessionDto(
            rt.Id,
            rt.CreatedAt,
            IsCurrentSession: rt.Token == _userContext.RefreshToken
        ));
        
        return Result<IEnumerable<ActiveSessionDto>>.Success(activeSessions);
    }
    
    public async Task<Result<string>> RevokeSessionAsync(Guid tokenId, CancellationToken ct = default)
    {
        var refreshToken = await _tokenRepository.GetRefreshTokenByIdAsync(tokenId, ct);

        if (refreshToken == null || refreshToken.UserId != _userContext.UserId || refreshToken.IsExpired)
        {
            return Result<string>.Failure(UserErrors.InvalidRefreshToken);
        }

        if (refreshToken.Token == _userContext.RefreshToken)
        {
            return Result<string>.Failure(UserErrors.CannotRevokeCurrentSession);
        }

        refreshToken.ExpiresAt = DateTime.UtcNow;
        await _tokenRepository.DeleteRefreshTokenAsync(refreshToken, ct);

        return Result<string>.Success("Session revoked successfully.");
    }

    private async Task<string> CreateTokenAsync(UserDto user)
    {
        int expirationMinutes = int.Parse(_configuration["JWT_TOKEN_SETTINGS:EXPIRATION_MINUTES"]);
        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var token = CreateJwtToken(
            await CreateClaimsAsync(user),
            CreateSigningCredentials(),
            expiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials, DateTime expiration) =>
        new(
            _configuration["JWT_TOKEN_SETTINGS:VALID_ISSUER"],
            _configuration["JWT_TOKEN_SETTINGS:VALID_AUDIENCE"],
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

    private async Task<List<Claim>> CreateClaimsAsync(UserDto user)
    {
        var jwtSub = _configuration["JwtTokenSettings:JwtRegisteredClaimNamesSub"];

        var claims = new List<Claim>
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var userEntity = await _userManager.FindByIdAsync(user.Id.ToString());

        if (userEntity != null)
        {
            var roles = await _userManager.GetRolesAsync(userEntity);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return claims;
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var symmetricSecurityKey = _configuration["JWT_TOKEN_SETTINGS:SYMMETRIC_SECURITY_KEY"];

        return new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricSecurityKey)),
            SecurityAlgorithms.HmacSha256
        );
    }
}