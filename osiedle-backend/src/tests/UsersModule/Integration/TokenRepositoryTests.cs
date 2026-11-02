using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;
using Users.Infrastructure.Data;

namespace UserModule.Integration;

public class TokenRepositoryTests : IntegrationTestBase
{
    private static RefreshToken NewToken(Guid userId, string token, DateTime? expiresAt = null) =>
        RefreshToken.Create(
            RefreshTokenId.New(),
            token,
            UserId.From(userId),
            expiresAt ?? DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow);

    // RefreshTokens.UserId is a foreign key, so a domain user row must exist first.
    private static async Task<Guid> CreateUserAsync(IServiceScope scope, string email)
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = UserId.New();
        db.Users.Add(User.Create(id, Name.Create("Jan", "Kowalski"), email));
        await db.SaveChangesAsync();
        return id.Value;
    }

    private static async Task SeedAsync(IServiceScope scope, params RefreshToken[] tokens)
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.RefreshTokens.AddRange(tokens);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Add_ThenReadBack_ShouldRoundTrip()
    {
        using var scope = CreateScope();
        var userId = await CreateUserAsync(scope, "owner1@example.com");
        var token = NewToken(userId, "abc-123");
        await SeedAsync(scope, token);

        var repository = scope.ServiceProvider.GetRequiredService<ITokenWriteRepository>();
        var loaded = await repository.GetByTokenForUpdateAsync("abc-123");

        loaded.Should().NotBeNull();
        loaded!.Token.Should().Be("abc-123");
        loaded.UserId.Should().Be(UserId.From(userId));
        loaded.ExpiresAt.Should().Be(token.ExpiresAt);
    }

    [Fact]
    public async Task GetByTokenForUpdateAsync_ShouldReturnNull_WhenTokenIsUnknown()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITokenWriteRepository>();

        var loaded = await repository.GetByTokenForUpdateAsync("missing-token");

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_ShouldReturnNull_WhenTokenIsUnknown()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITokenReadRepository>();

        var loaded = await repository.GetByTokenAsync("missing-token");

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnToken_WhenIdIsKnown()
    {
        using var seedScope = CreateScope();
        var userId = await CreateUserAsync(seedScope, "owner2@example.com");
        var token = NewToken(userId, "abc-123");
        await SeedAsync(seedScope, token);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITokenReadRepository>();

        var loaded = await repository.GetByIdAsync(token.Id);
        var missing = await repository.GetByIdAsync(RefreshTokenId.From(Guid.NewGuid()));

        loaded.Should().NotBeNull();
        loaded!.Token.Should().Be("abc-123");
        missing.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_ShouldReturnOnlyActiveTokensOfGivenUser()
    {
        using var seedScope = CreateScope();
        var userId = await CreateUserAsync(seedScope, "owner3@example.com");
        var foreignUserId = await CreateUserAsync(seedScope, "foreign@example.com");

        // RefreshToken.Create rejects past expiry, so the inactive own token is
        // modelled as a revoked one instead of an expired one.
        var activeOwn = NewToken(userId, "active-own");
        var revokedOwn = NewToken(userId, "revoked-own");
        revokedOwn.Revoke(UserId.From(userId), DateTime.UtcNow);
        var activeForeign = NewToken(foreignUserId, "active-foreign");
        await SeedAsync(seedScope, activeOwn, revokedOwn, activeForeign);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITokenReadRepository>();

        var active = await repository.GetActiveTokensByUserIdAsync(UserId.From(userId));

        active.Should().ContainSingle(rt => rt.Token == "active-own");
    }
}
