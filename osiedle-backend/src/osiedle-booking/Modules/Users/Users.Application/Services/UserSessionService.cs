using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Services.Tokens;
using Users.Application.DTOs;
using Users.Application.Errors;
using Users.Domain.Entity.RefreshToken.ValueObjects;

namespace Users.Application.Services;

public class UserSessionService : IUserSessionService 
{
    private readonly ITokenReadRepository _tokenReadRepository;
    private readonly ITokenWriteRepository _tokenWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public UserSessionService(ITokenReadRepository tokenReadRepository,
        ITokenWriteRepository tokenWriteRepository, IUnitOfWork unitOfWork)
    {
        _tokenReadRepository = tokenReadRepository;
        _tokenWriteRepository = tokenWriteRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<IReadOnlyCollection<ActiveSessionDto>>> GetActiveSessionsAsync(UserId userId,
        string currentRefreshToken, CancellationToken ct = default)
    {
        var refreshTokens = await _tokenReadRepository.
            GetActiveTokensByUserIdAsync(userId, ct);
        
        var activeSessions = refreshTokens
            .Select(rt => new ActiveSessionDto(
                rt.Id,
                rt.CreatedAt,
                IsCurrentSession: rt.Token == currentRefreshToken
            ))
            .ToList();
    
        return Result<IReadOnlyCollection<ActiveSessionDto>>.Success(activeSessions);
    }

    public async Task<Result> RevokeSessionAsync(UserId userId, RefreshTokenId tokenId,
        string currentRefreshToken, CancellationToken ct = default)
    {
        // Грузим отслеживаемую строку (write-repo): мутация Revoke должна попасть в SaveChanges.
        var refreshToken = await _tokenWriteRepository.GetByIdForUpdateAsync(tokenId, ct);

        if (refreshToken == null || refreshToken.UserId != userId || !refreshToken.IsActive)
            return Result.Failure(TokenErrors.InvalidRefreshToken);

        if (refreshToken.Token == currentRefreshToken)
            return Result.Failure(TokenErrors.CannotRevokeCurrentSession);
        
        refreshToken.Revoke(userId, DateTime.UtcNow);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success();
    }

    public async Task<Result> RevokeCurrentSessionAsync(UserId userId, string currentRefreshToken,
        CancellationToken ct = default)
    {
        var refreshToken = await _tokenWriteRepository.GetByTokenForUpdateAsync(currentRefreshToken, ct);

        if (refreshToken == null || refreshToken.UserId != userId || !refreshToken.IsActive)
            return Result.Failure(TokenErrors.InvalidRefreshToken);

        refreshToken.Revoke(userId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}