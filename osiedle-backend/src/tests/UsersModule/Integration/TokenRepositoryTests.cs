using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Repositories;
using Users.Domain.Entity.RefreshToken;
using Users.Infrastructure.Data;

namespace UserModule.Integration;

public class TokenRepositoryTests : IntegrationTestBase
{
    private static RefreshToken NewToken(Guid userId, string token, DateTime? expiresAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Token = token,
        UserId = userId,
        CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30)
    };

    [Fact]
    public async Task AddRefreshToken_ThenGetByToken_ShouldRoundTrip()
    {
        var userId = Guid.NewGuid();
        var token = NewToken(userId, "abc-123");

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
            await repository.AddRefreshTokenAsync(token);
        }

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var loaded = await db.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.Token == "abc-123");

        loaded.UserId.Should().Be(userId);
        loaded.ExpiresAt.Should().Be(token.ExpiresAt);
    }

    [Fact]
    public async Task GetRefreshTokenAsync_ShouldReturnNull_WhenTokenIsUnknown()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();

        var loaded = await repository.GetRefreshTokenAsync("missing-token");

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task GetRefreshTokenByIdAsync_ShouldReturnToken_WhenIdIsKnown()
    {
        var token = NewToken(Guid.NewGuid(), "abc-123");

        using (var scope = CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ITokenRepository>().AddRefreshTokenAsync(token);
        }

        using var verifyScope = CreateScope();
        var repository = verifyScope.ServiceProvider.GetRequiredService<ITokenRepository>();

        var loaded = await repository.GetRefreshTokenByIdAsync(token.Id);
        var missing = await repository.GetRefreshTokenByIdAsync(Guid.NewGuid());

        loaded.Should().NotBeNull();
        loaded!.Token.Should().Be("abc-123");
        missing.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveRefreshTokens_ShouldReturnOnlyActiveTokensOfGivenUser()
    {
        var userId = Guid.NewGuid();
        var activeOwn = NewToken(userId, "active-own");
        var expiredOwn = NewToken(userId, "expired-own", DateTime.UtcNow.AddDays(-1));
        var activeForeign = NewToken(Guid.NewGuid(), "active-foreign");

        using (var scope = CreateScope())
        {
            var addRepository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
            await addRepository.AddRefreshTokenAsync(activeOwn);
            await addRepository.AddRefreshTokenAsync(expiredOwn);
            await addRepository.AddRefreshTokenAsync(activeForeign);
        }

        using var verifyScope = CreateScope();
        var repository = verifyScope.ServiceProvider.GetRequiredService<ITokenRepository>();

        var active = await repository.GetActiveRefreshTokensAsync(userId);

        active.Should().ContainSingle(rt => rt.Token == "active-own");
    }

    [Fact]
    public async Task UpdateRefreshToken_ShouldPersistNewExpiry()
    {
        var token = NewToken(Guid.NewGuid(), "abc-123");

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
            await repository.AddRefreshTokenAsync(token);

            token.ExpiresAt = DateTime.UtcNow.AddDays(-1);
            await repository.UpdateRefreshTokenAsync(token);
        }

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reloaded = await db.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.Token == "abc-123");

        reloaded.IsExpired.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRefreshToken_ShouldRemoveToken()
    {
        var token = NewToken(Guid.NewGuid(), "abc-123");

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
            await repository.AddRefreshTokenAsync(token);
            await repository.DeleteRefreshTokenAsync(token);
        }

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var remaining = await db.RefreshTokens.AsNoTracking().CountAsync(rt => rt.Token == "abc-123");
        remaining.Should().Be(0);
    }
}
