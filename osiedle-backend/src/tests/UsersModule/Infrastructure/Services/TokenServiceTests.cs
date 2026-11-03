using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Tokens;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;
using Users.Infrastructure.Options;
using Users.Infrastructure.Services.Tokens;

namespace UserModule.Infrastructure.Services;

public class TokenServiceTests
{
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock = new();
    private readonly Mock<ITokenWriteRepository> _tokenWriteMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IIdentityAccountService> _identityAccountMock = new();
    private readonly Mock<IUserReadRepository> _userReadMock = new();
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _jwtGeneratorMock
            .Setup(g => g.GenerateAccessToken(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>()))
            .Returns("access-token");
        _jwtGeneratorMock.Setup(g => g.GenerateRefreshToken()).Returns("refresh-token");

        _sut = new TokenService(
            _jwtGeneratorMock.Object,
            _tokenWriteMock.Object,
            _unitOfWorkMock.Object,
            Options.Create(new JwtSettings
            {
                Secret = new string('k', 64),
                Issuer = "test-issuer",
                Audience = "test-audience",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7
            }),
            _identityAccountMock.Object,
            _userReadMock.Object);
    }

    private static IdentityUserDto IdentityUser(Guid id, string email) =>
        new(id, email, new[] { "User" });

    [Fact]
    public async Task GenerateTokensAsync_Success_ReturnsTokens_AndPersistsRefreshToken()
    {
        var userId = Guid.NewGuid();
        const string email = "jan@example.com";
        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(IdentityUser(userId, email)));

        var result = await _sut.GenerateTokensAsync(
            UserId.From(userId), email, new[] { "User" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");

        _tokenWriteMock.Verify(r => r.Add(
            It.Is<RefreshToken>(rt =>
                rt.Token == "refresh-token" &&
                rt.UserId == UserId.From(userId) &&
                rt.ExpiresAt > DateTime.UtcNow),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateTokensAsync_UsesRefreshTokenExpiryFromSettings()
    {
        var userId = Guid.NewGuid();
        const string email = "jan@example.com";
        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(IdentityUser(userId, email)));

        await _sut.GenerateTokensAsync(UserId.From(userId), email, new[] { "User" }, CancellationToken.None);

        // JwtSettings.RefreshTokenExpiryDays = 7 in this fixture.
        _tokenWriteMock.Verify(r => r.Add(
            It.Is<RefreshToken>(rt =>
                rt.ExpiresAt >= DateTime.UtcNow.AddDays(7).AddSeconds(-5)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateTokensByRefreshTokenAsync_Success_RotatesTokens_AndPersistsNewRefreshToken()
    {
        var userId = Guid.NewGuid();
        const string email = "jan@example.com";
        var domainUser = User.Create(UserId.From(userId), Name.Create("Jan", "Kowalski"), email);
        _userReadMock
            .Setup(r => r.GetUserByRefreshTokenAsync("old-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainUser);
        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(IdentityUser(userId, email)));

        var result = await _sut.GenerateTokensByRefreshTokenAsync("old-token", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");

        _tokenWriteMock.Verify(r => r.Add(
            It.Is<RefreshToken>(rt =>
                rt.Token == "refresh-token" &&
                rt.UserId == UserId.From(userId)),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
