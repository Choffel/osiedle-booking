using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;
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
    private readonly AuthService _authService;

    public AuthenticateTests()
    {
        _userContextMock.Setup(c => c.IpAddress).Returns("127.0.0.1");

        _authService = new AuthService(
            _identityCredentialsMock.Object,
            _identityAccountMock.Object,
            _identityEmailMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            _roleServiceMock.Object,
            _tokenServiceMock.Object,
            _userContextMock.Object);
    }

    private static IdentityUserDto KnownUser(Guid? id = null) => new(
        Id: id ?? Guid.NewGuid(),
        UserName: "testuser",
        Email: "test@example.com",
        EmailConfirmed: true,
        Succeeded: true);

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var request = new AuthRequest("test@example.com", "P@ssw0rd1");
        var identityUser = KnownUser();
        var tokens = new TokenResponseDto("access_token", "refresh_token");

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(identityUser));
        _identityCredentialsMock.Setup(s => s.CheckPasswordAsync(identityUser.Id, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("Password is correct"));
        _tokenServiceMock.Setup(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        var result = await _authService.AuthenticateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");

        _tokenServiceMock.Verify(s => s.GetTokens(
            It.Is<UserDto>(u =>
                u.Id == identityUser.Id &&
                u.UserName == identityUser.UserName &&
                u.Email == identityUser.Email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenUserNotFound()
    {
        // Неизвестный email и неверный пароль обязаны отдавать одно и то же сообщение,
        // иначе ответ раскрывает, зарегистрирован ли адрес в системе.
        var request = new AuthRequest("missing@example.com", "P@ssw0rd1");

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Failure(UserErrors.NotFound));

        var result = await _authService.AuthenticateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
        _tokenServiceMock.Verify(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenCredentialsServiceRejectsPassword()
    {
        var request = new AuthRequest("test@example.com", "WrongPassword1");
        var identityUser = KnownUser();

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(identityUser));
        _identityCredentialsMock.Setup(s => s.CheckPasswordAsync(identityUser.Id, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(UserErrors.InvalidCredentials));

        var result = await _authService.AuthenticateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
        _tokenServiceMock.Verify(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenAccountIsLockedOut()
    {
        var request = new AuthRequest("test@example.com", "P@ssw0rd1");
        var identityUser = KnownUser();

        _identityAccountMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(identityUser));
        _identityCredentialsMock.Setup(s => s.CheckPasswordAsync(identityUser.Id, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(UserErrors.LockedOut));

        var result = await _authService.AuthenticateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
        _tokenServiceMock.Verify(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
