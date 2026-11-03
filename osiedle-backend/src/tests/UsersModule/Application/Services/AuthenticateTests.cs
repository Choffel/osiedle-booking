using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Abstractions.Services.Tokens;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;
using Users.Application.Errors;
using Users.Application.Services;

namespace UserModule.Application.Services;

public class AuthenticateTests
{
    private readonly Mock<IIdentityCredentialsService> _identityCredentialsMock = new();
    private readonly Mock<IIdentityAccountService> _identityAccountMock = new();
    private readonly Mock<IIdentityEmailService> _identityEmailMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthService _authService;

    public AuthenticateTests()
    {
        _userContextMock.Setup(c => c.IpAddress).Returns("127.0.0.1");

        _authService = new AuthService(
            _identityCredentialsMock.Object,
            _identityAccountMock.Object,
            _identityEmailMock.Object,
            _loggerMock.Object,
            _roleServiceMock.Object,
            _tokenServiceMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    private static IdentityUserDto KnownUser(Guid? id = null) => new(
        Id: id ?? Guid.NewGuid(),
        Email: "test@example.com",
        Roles: new[] { "User" });

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var request = new AuthRequest("test@example.com", "P@ssw0rd1");
        var identityUser = KnownUser();
        var tokens = new TokenResponseDto("access_token", "refresh_token");

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(identityUser));
        _identityCredentialsMock
            .Setup(s => s.CheckPasswordAsync(UserId.From(identityUser.Id), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _tokenServiceMock
            .Setup(s => s.GenerateTokensAsync(
                UserId.From(identityUser.Id), identityUser.Email, identityUser.Roles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TokenResponseDto>.Success(tokens));

        var result = await _authService.AuthenticateAsync(request);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");

        _tokenServiceMock.Verify(s => s.GenerateTokensAsync(
            It.Is<UserId>(u => u == UserId.From(identityUser.Id)),
            identityUser.Email,
            It.Is<IReadOnlyCollection<string>>(r => r.Contains("User")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenUserNotFound()
    {
        var request = new AuthRequest("missing@example.com", "P@ssw0rd1");

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Failure(IdentityErrors.NotFound));

        var result = await _authService.AuthenticateAsync(request);

        // The unknown-email answer must be indistinguishable from a wrong password.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
        _tokenServiceMock.Verify(
            s => s.GenerateTokensAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenCredentialsServiceRejectsPassword()
    {
        var request = new AuthRequest("test@example.com", "WrongPassword1");
        var identityUser = KnownUser();

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(identityUser));
        _identityCredentialsMock
            .Setup(s => s.CheckPasswordAsync(UserId.From(identityUser.Id), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(IdentityErrors.InvalidCredentials));

        var result = await _authService.AuthenticateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
        _tokenServiceMock.Verify(
            s => s.GenerateTokensAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldPropagateLockout_WhenAccountIsLockedOut()
    {
        var request = new AuthRequest("test@example.com", "P@ssw0rd1");
        var identityUser = KnownUser();

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(identityUser));
        _identityCredentialsMock
            .Setup(s => s.CheckPasswordAsync(UserId.From(identityUser.Id), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(IdentityErrors.LockedOut));

        var result = await _authService.AuthenticateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.LockedOut);
        _tokenServiceMock.Verify(
            s => s.GenerateTokensAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
