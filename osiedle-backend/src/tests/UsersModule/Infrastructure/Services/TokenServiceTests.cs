using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.DTOs;
using Users.Domain.Entity.RefreshToken;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Services;

namespace UserModule.Infrastructure.Services;

public class TokenServiceTests
{
    private readonly Mock<ITokenRepository> _tokenRepositoryMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_TOKEN_SETTINGS:SYMMETRIC_SECURITY_KEY"] = new string('k', 64),
                ["JWT_TOKEN_SETTINGS:VALID_ISSUER"] = "test-issuer",
                ["JWT_TOKEN_SETTINGS:VALID_AUDIENCE"] = "test-audience"
            })
            .Build();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        _sut = new TokenService(
            configuration,
            _tokenRepositoryMock.Object,
            _userManagerMock.Object,
            Mock.Of<ILogger<TokenService>>(),
            _userContextMock.Object);
    }

    private void SetupUserLookup(Guid userId)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "jan",
            Email = "jan@example.com"
        };
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());
    }

    [Fact]
    public async Task GetTokens_ShouldReturnTokens_AndStoreRefreshToken()
    {
        var userId = Guid.NewGuid();
        SetupUserLookup(userId);
        var userDto = new UserDto(userId, "jan", "jan@example.com");

        var tokens = await _sut.GetTokens(userDto, CancellationToken.None);

        tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokens.AccessToken.Split('.').Should().HaveCount(3);
        tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
        _tokenRepositoryMock.Verify(r => r.AddRefreshTokenAsync(
            It.Is<RefreshToken>(rt => rt.UserId == userId && rt.Token == tokens.RefreshToken)), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshToken_ShouldFail_WhenTokenIsUnknown()
    {
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("missing-token");
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("missing-token"))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.RevokeRefreshTokenAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
        _tokenRepositoryMock.Verify(r => r.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeRefreshToken_ShouldFail_WhenTokenBelongsToAnotherUser()
    {
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("token");
        _userContextMock.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("token"))
            .ReturnsAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "token",
                UserId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(10)
            });

        var result = await _sut.RevokeRefreshTokenAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RevokeRefreshToken_ShouldFail_WhenTokenIsExpired()
    {
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("token");
        var currentUserId = Guid.NewGuid();
        _userContextMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("token"))
            .ReturnsAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "token",
                UserId = currentUserId,
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            });

        var result = await _sut.RevokeRefreshTokenAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RevokeRefreshToken_ShouldExpireToken_WhenTokenIsValid()
    {
        var currentUserId = Guid.NewGuid();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "token",
            UserId = currentUserId,
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("token");
        _userContextMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("token")).ReturnsAsync(refreshToken);

        var result = await _sut.RevokeRefreshTokenAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        refreshToken.ExpiresAt.Should().BeOnOrBefore(DateTime.UtcNow);
        _tokenRepositoryMock.Verify(r => r.UpdateRefreshTokenAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task GetNewAccessToken_ShouldFail_WhenTokenIsUnknown()
    {
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("missing-token");
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("missing-token"))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.GetNewAccessTokenAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
        _tokenRepositoryMock.Verify(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task GetNewAccessToken_ShouldFail_WhenTokenIsExpired()
    {
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("token");
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("token"))
            .ReturnsAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "token",
                UserId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            });

        var result = await _sut.GetNewAccessTokenAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task GetNewAccessToken_ShouldFailWithoutSideEffects_WhenUserIsMissing()
    {
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("token");
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("token"))
            .ReturnsAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "token",
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(10)
            });
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.GetNewAccessTokenAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
        _tokenRepositoryMock.Verify(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        _tokenRepositoryMock.Verify(r => r.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task GetNewAccessToken_ShouldRotateTokens_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var oldToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "old-token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("old-token");
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenAsync("old-token")).ReturnsAsync(oldToken);
        SetupUserLookup(userId);

        var result = await _sut.GetNewAccessTokenAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Split('.').Should().HaveCount(3);
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshToken.Should().NotBe("old-token");
        oldToken.ExpiresAt.Should().BeOnOrBefore(DateTime.UtcNow);
        _tokenRepositoryMock.Verify(r => r.UpdateRefreshTokenAsync(oldToken), Times.Once);
        _tokenRepositoryMock.Verify(r => r.AddRefreshTokenAsync(
            It.Is<RefreshToken>(rt => rt.UserId == userId && rt.Token == result.Value.RefreshToken)), Times.Once);
    }

    [Fact]
    public async Task GetActiveSessions_ShouldMarkCurrentSession()
    {
        var currentUserId = Guid.NewGuid();
        var currentToken = Guid.NewGuid();
        var otherToken = Guid.NewGuid();
        _userContextMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("current-token");
        _tokenRepositoryMock.Setup(r => r.GetActiveRefreshTokensAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>
            {
                new()
                {
                    Id = currentToken,
                    Token = "current-token",
                    UserId = currentUserId,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    ExpiresAt = DateTime.UtcNow.AddDays(10)
                },
                new()
                {
                    Id = otherToken,
                    Token = "other-token",
                    UserId = currentUserId,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    ExpiresAt = DateTime.UtcNow.AddDays(10)
                }
            });

        var result = await _sut.GetActiveSessionsAsync();

        result.IsSuccess.Should().BeTrue();
        var sessions = result.Value!.ToList();
        sessions.Should().HaveCount(2);
        sessions.Single(s => s.TokenId == currentToken).IsCurrentSession.Should().BeTrue();
        sessions.Single(s => s.TokenId == otherToken).IsCurrentSession.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeSession_ShouldFail_WhenSessionIsUnknown()
    {
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.RevokeSessionAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
        _tokenRepositoryMock.Verify(r => r.DeleteRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSession_ShouldFail_WhenSessionBelongsToAnotherUser()
    {
        _userContextMock.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "foreign-token",
                UserId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(10)
            });

        var result = await _sut.RevokeSessionAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RevokeSession_ShouldFail_WhenSessionIsExpired()
    {
        var currentUserId = Guid.NewGuid();
        _userContextMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "expired-token",
                UserId = currentUserId,
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            });

        var result = await _sut.RevokeSessionAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RevokeSession_ShouldFail_WhenTryingToRevokeCurrentSession()
    {
        var currentUserId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        _userContextMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("current-token");
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenByIdAsync(tokenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken
            {
                Id = tokenId,
                Token = "current-token",
                UserId = currentUserId,
                ExpiresAt = DateTime.UtcNow.AddDays(10)
            });

        var result = await _sut.RevokeSessionAsync(tokenId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.CannotRevokeCurrentSession);
        _tokenRepositoryMock.Verify(r => r.DeleteRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSession_ShouldDeleteSession_WhenSessionIsValid()
    {
        var currentUserId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var session = new RefreshToken
        {
            Id = tokenId,
            Token = "other-token",
            UserId = currentUserId,
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };
        _userContextMock.SetupGet(c => c.UserId).Returns(currentUserId);
        _userContextMock.SetupGet(c => c.RefreshToken).Returns("current-token");
        _tokenRepositoryMock.Setup(r => r.GetRefreshTokenByIdAsync(tokenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _sut.RevokeSessionAsync(tokenId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Session revoked successfully.");
        _tokenRepositoryMock.Verify(r => r.DeleteRefreshTokenAsync(session, It.IsAny<CancellationToken>()), Times.Once);
    }
}
