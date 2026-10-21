using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;
using Users.Application.Services;

namespace UserModule.Application.Services;

public class AuthenticateTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly UserService _userService;
    private readonly IUserContext _userContext;

    public AuthenticateTests()
    {
        _userService = new UserService(
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            _roleServiceMock.Object,
            _tokenServiceMock.Object,
            _userContext);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new AuthRequest("test@example.com", "P@ssw0rd1");

        var identityUser = new IdentityUserDto(
            Id: Guid.NewGuid(),
            UserName: "testuser",
            Email: "test@example.com",
            EmailConfirmed: true,
            Succeeded: true);

        var tokens = new TokenResponseDto("access_token", "refresh_token");

        _identityServiceMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(identityUser);

        _identityServiceMock.Setup(s => s.CheckPasswordAsync(identityUser.Id, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _tokenServiceMock.Setup(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        // Act
        var result = await _userService.AuthenticateAsync(request);

        // Assert
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
        // Arrange
        var request = new AuthRequest("missing@example.com", "P@ssw0rd1");

        _identityServiceMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserDto)null!);

        // Act
        var result = await _userService.AuthenticateAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid Email or Password");
        _tokenServiceMock.Verify(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenEmailNotConfirmed()
    {
        // Arrange
        var request = new AuthRequest("test@example.com", "P@ssw0rd1");

        var identityUser = new IdentityUserDto(
            Id: Guid.NewGuid(),
            UserName: "testuser",
            Email: "test@example.com",
            EmailConfirmed: false,
            Succeeded: true);

        _identityServiceMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(identityUser);

        // Act
        var result = await _userService.AuthenticateAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email not confirmed");
        _tokenServiceMock.Verify(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenPasswordIsInvalid()
    {
        // Arrange
        var request = new AuthRequest("test@example.com", "WrongPassword1");

        var identityUser = new IdentityUserDto(
            Id: Guid.NewGuid(),
            UserName: "testuser",
            Email: "test@example.com",
            EmailConfirmed: true,
            Succeeded: true);

        _identityServiceMock.Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(identityUser);

        _identityServiceMock.Setup(s => s.CheckPasswordAsync(identityUser.Id, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.AuthenticateAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid Email or Password");
        _tokenServiceMock.Verify(s => s.GetTokens(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
