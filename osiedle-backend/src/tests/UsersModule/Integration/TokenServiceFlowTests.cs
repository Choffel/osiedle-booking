using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.RefreshToken;
using Users.Infrastructure.Data;

namespace UserModule.Integration;

public class TokenServiceFlowTests : IntegrationTestBase
{
    private static RefreshToken NewRefreshToken(Guid userId, string token, DateTime? expiresAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Token = token,
        UserId = userId,
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30)
    };

    [Fact]
    public async Task GetTokens_ShouldPersistRefreshToken()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var tokens = await tokenService.GetTokens(new UserDto(userId, "jan", email), CancellationToken.None);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await db.RefreshTokens.AsNoTracking()
            .SingleOrDefaultAsync(rt => rt.Token == tokens.RefreshToken);

        stored.Should().NotBeNull();
        stored!.UserId.Should().Be(userId);
        stored.IsExpired.Should().BeFalse();
    }

    [Fact]
    public async Task GetNewAccessToken_ShouldRotateTokens()
    {
        const string email = "jan@example.com";
        const string oldTokenValue = "old-refresh-token";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var tokenRepository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
        var oldToken = NewRefreshToken(userId, oldTokenValue);
        await tokenRepository.AddRefreshTokenAsync(oldToken);

        UserContext.UserId = userId;
        UserContext.RefreshToken = oldTokenValue;

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var result = await tokenService.GetNewAccessTokenAsync();

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value!.AccessToken.Split('.').Should().HaveCount(3);
        result.Value.RefreshToken.Should().NotBe(oldTokenValue);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = await db.RefreshTokens.AsNoTracking().Where(rt => rt.UserId == userId).ToListAsync();

        tokens.Should().HaveCount(2, "старый токен должен остаться в БД в истёкшем виде, а новый добавиться");
        tokens.Single(rt => rt.Token == oldTokenValue).IsExpired.Should().BeTrue();
        tokens.Single(rt => rt.Token == result.Value.RefreshToken).IsExpired.Should().BeFalse();
    }

    [Fact]
    public async Task GetNewAccessToken_ShouldFail_WhenTokenIsUnknown()
    {
        UserContext.RefreshToken = "missing-token";

        using var scope = CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var result = await tokenService.GetNewAccessTokenAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.RefreshTokens.AsNoTracking().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RevokeRefreshToken_ShouldPersistExpiry()
    {
        const string email = "jan@example.com";
        const string refreshTokenValue = "refresh-token-to-revoke";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var tokenRepository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
        await tokenRepository.AddRefreshTokenAsync(NewRefreshToken(userId, refreshTokenValue));

        UserContext.UserId = userId;
        UserContext.RefreshToken = refreshTokenValue;

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var result = await tokenService.RevokeRefreshTokenAsync();

        result.IsSuccess.Should().BeTrue(result.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await db.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.Token == refreshTokenValue);
        stored.IsExpired.Should().BeTrue("ревок должен сохраняться в БД");
    }

    [Fact]
    public async Task GetActiveSessions_ShouldListOnlyCurrentUsersSessions()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var tokenRepository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
        var currentToken = NewRefreshToken(userId, "current-token");
        var otherToken = NewRefreshToken(userId, "other-token");
        await tokenRepository.AddRefreshTokenAsync(currentToken);
        await tokenRepository.AddRefreshTokenAsync(otherToken);
        await tokenRepository.AddRefreshTokenAsync(NewRefreshToken(Guid.NewGuid(), "foreign-token"));

        UserContext.UserId = userId;
        UserContext.RefreshToken = "current-token";

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var result = await tokenService.GetActiveSessionsAsync();

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        var sessions = result.Value!.ToList();
        sessions.Should().HaveCount(2, "сессии других пользователей не должны попадать в выдачу");
        sessions.Single(s => s.TokenId == currentToken.Id).IsCurrentSession.Should().BeTrue();
        sessions.Single(s => s.TokenId == otherToken.Id).IsCurrentSession.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeSession_ShouldDeleteOtherSession_ButRefuseCurrent()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var tokenRepository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
        var currentToken = NewRefreshToken(userId, "current-token");
        var otherToken = NewRefreshToken(userId, "other-token");
        await tokenRepository.AddRefreshTokenAsync(currentToken);
        await tokenRepository.AddRefreshTokenAsync(otherToken);

        UserContext.UserId = userId;
        UserContext.RefreshToken = "current-token";

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var refuseResult = await tokenService.RevokeSessionAsync(currentToken.Id);
        refuseResult.IsSuccess.Should().BeFalse();
        refuseResult.Error.Should().Be(UserErrors.CannotRevokeCurrentSession);

        var revokeResult = await tokenService.RevokeSessionAsync(otherToken.Id);
        revokeResult.IsSuccess.Should().BeTrue(revokeResult.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = await db.RefreshTokens.AsNoTracking().Where(rt => rt.UserId == userId).ToListAsync();
        tokens.Should().ContainSingle(rt => rt.Token == "current-token");
        tokens.Should().NotContain(rt => rt.Token == "other-token");
    }
}
