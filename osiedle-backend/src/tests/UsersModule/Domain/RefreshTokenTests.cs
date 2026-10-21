using FluentAssertions;
using Users.Domain.Entity.RefreshToken;

namespace UserModule.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void IsExpired_ShouldBeFalse_WhenExpiresAtInFuture()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldBeTrue_WhenExpiresAtInPast()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddMinutes(-5) };

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldBeTrue_WhenExpiresAtJustPassed()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) };

        token.IsExpired.Should().BeTrue();
    }
}
