using FluentAssertions;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace UserModule.Domain;

public class RefreshTokenTests
{
    private static RefreshToken CreateToken(DateTime expiresAt, DateTime? utcNow = null) =>
        RefreshToken.Create(
            RefreshTokenId.New(),
            $"token-{Guid.NewGuid():N}",
            UserId.New(),
            expiresAt,
            utcNow ?? DateTime.UtcNow);

    [Fact]
    public void IsActive_ShouldBeTrue_WhenNotRevokedAndNotExpired()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(5));

        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRejectExpirationInPast()
    {
        var now = DateTime.UtcNow;

        var act = () => CreateToken(now.AddSeconds(-1), now);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Revoke_ShouldDeactivateToken_AndKeepFirstRevocation()
    {
        var owner = UserId.New();
        var token = RefreshToken.Create(
            RefreshTokenId.New(), "some-token", owner, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        var revokedAt = DateTime.UtcNow;

        token.Revoke(owner, revokedAt);

        token.IsActive.Should().BeFalse();
        token.RevokedAt.Should().Be(revokedAt);

        // A second revoke must be a no-op and keep the original timestamp.
        token.Revoke(owner, revokedAt.AddMinutes(1));
        token.RevokedAt.Should().Be(revokedAt);
    }

    [Fact]
    public void Revoke_ShouldThrow_WhenTokenBelongsToAnotherUser()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(5));

        var act = () => token.Revoke(UserId.New(), DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
