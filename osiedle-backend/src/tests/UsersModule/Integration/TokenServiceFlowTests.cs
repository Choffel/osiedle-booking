using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Services.Tokens;
using Users.Application.DTOs;
using Users.Application.Errors;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Infrastructure.Data;
using Users.Application.Abstractions.Services.Identity;

namespace UserModule.Integration;

public class TokenServiceFlowTests : IntegrationTestBase
{
    private static RefreshToken NewRefreshToken(Guid userId, string token, DateTime? expiresAt = null) =>
        RefreshToken.Create(
            RefreshTokenId.New(),
            token,
            UserId.From(userId),
            expiresAt ?? DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow);

    private static async Task SeedAsync(IServiceScope scope, params RefreshToken[] tokens)
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.RefreshTokens.AddRange(tokens);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GenerateTokens_ShouldIssueJwtAndPersistRefreshToken()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var result = await tokenService.GenerateTokensAsync(
            UserId.From(userId), email, new[] { "User" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.AccessToken.Split('.').Should().HaveCount(3);
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await db.RefreshTokens.AsNoTracking()
            .SingleOrDefaultAsync(rt => rt.Token == result.Value.RefreshToken);

        stored.Should().NotBeNull();
        stored!.UserId.Should().Be(UserId.From(userId));
        stored.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateTokensByRefreshToken_ShouldRotateTokens_RevokingOldRow()
    {
        const string email = "jan@example.com";
        const string oldTokenValue = "old-refresh-token";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        await SeedAsync(scope, NewRefreshToken(userId, oldTokenValue));

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var result = await tokenService.GenerateTokensByRefreshTokenAsync(oldTokenValue, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.RefreshToken.Should().NotBe(oldTokenValue);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = await db.RefreshTokens.AsNoTracking()
            .Where(rt => rt.UserId == UserId.From(userId))
            .ToListAsync();

        tokens.Should().HaveCount(2);
        tokens.Single(rt => rt.Token == oldTokenValue).RevokedAt.Should().NotBeNull(
            "использованный токен должен отзываться при ротации");
        tokens.Single(rt => rt.Token == result.Value.RefreshToken).RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task GenerateTokensByRefreshToken_ShouldFail_WhenTokenIsUnknown()
    {
        using var scope = CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var result = await tokenService.GenerateTokensByRefreshTokenAsync(
            "missing-token", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(TokenErrors.InvalidRefreshToken);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.RefreshTokens.AsNoTracking().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetActiveSessions_ShouldListOnlyCurrentUsersSessions()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var currentToken = NewRefreshToken(userId, "current-token");
        var otherToken = NewRefreshToken(userId, "other-token");
        var foreignAuthService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var foreignId = await RegisterUserAsync(foreignAuthService, "foreign@example.com", ValidPassword, "Obcy");
        await SeedAsync(scope, currentToken, otherToken, NewRefreshToken(foreignId, "foreign-token"));

        var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
        var result = await sessionService.GetActiveSessionsAsync(
            UserId.From(userId), "current-token", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        var sessions = ((Result<List<ActiveSessionDto>>)result).Value;
        sessions.Should().HaveCount(2, "сессии других пользователей не должны попадать в выдачу");
        sessions.Single(s => s.TokenId == currentToken.Id.Value).IsCurrentSession.Should().BeTrue();
        sessions.Single(s => s.TokenId == otherToken.Id.Value).IsCurrentSession.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeSession_ShouldRevokeOtherSession_ButKeepCurrent()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var currentToken = NewRefreshToken(userId, "current-token");
        var otherToken = NewRefreshToken(userId, "other-token");
        await SeedAsync(scope, currentToken, otherToken);

        var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();

        var result = await sessionService.RevokeSessionAsync(
            UserId.From(userId), otherToken.Id, "current-token", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = await db.RefreshTokens.AsNoTracking().Where(rt => rt.UserId == UserId.From(userId)).ToListAsync();
        tokens.Single(rt => rt.Token == "current-token").RevokedAt.Should().BeNull();
        tokens.Single(rt => rt.Token == "other-token").RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeSession_WithCurrentSessionToken_ShouldBeRefused()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var currentToken = NewRefreshToken(userId, "current-token");
        await SeedAsync(scope, currentToken);

        var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
        var result = await sessionService.RevokeSessionAsync(
            UserId.From(userId), currentToken.Id, "current-token", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(TokenErrors.CannotRevokeCurrentSession);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.RefreshTokens.AsNoTracking().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RevokeSession_WithUnknownTokenId_ShouldFail()
    {
        using var scope = CreateScope();
        var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();

        var result = await sessionService.RevokeSessionAsync(
            UserId.From(Guid.NewGuid()), RefreshTokenId.From(Guid.NewGuid()), "whatever", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(TokenErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RevokeCurrentSession_ShouldRevokeTokenFromHeader()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        await SeedAsync(scope, NewRefreshToken(userId, "logout-token"));

        var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
        var result = await sessionService.RevokeCurrentSessionAsync(
            UserId.From(userId), "logout-token", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.Token == "logout-token"))
            .RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeCurrentSession_WithUnknownToken_ShouldFail()
    {
        using var scope = CreateScope();
        var sessionService = scope.ServiceProvider.GetRequiredService<IUserSessionService>();

        var result = await sessionService.RevokeCurrentSessionAsync(
            UserId.From(Guid.NewGuid()), "missing-token", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(TokenErrors.InvalidRefreshToken);
    }
}
