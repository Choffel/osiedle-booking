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

public class ConfirmEmailTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly UserService _userService;

    public ConfirmEmailTests()
    {
        _userService = new UserService(
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            Mock.Of<ILogger<UserService>>(),
            _roleServiceMock.Object,
            _tokenServiceMock.Object,
            _userContextMock.Object);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenUserDoesNotExist()
    {
        var request = new ConfirmEmailRequest("ghost@example.com", "any-token");

        _identityServiceMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserDto?)null);

        var result = await _userService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email address");
        _identityServiceMock.Verify(
            s => s.ConfirmEmailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenTokenIsInvalid()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "bad-token");

        _identityServiceMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "jan", request.Email, false, true));
        _identityServiceMock
            .Setup(s => s.ConfirmEmailAsync(userId, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _userService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid or expired confirmation token");
        _identityServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenUserRoleIsMissing()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "good-token");

        _identityServiceMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "jan", request.Email, false, true));
        _identityServiceMock
            .Setup(s => s.ConfirmEmailAsync(userId, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _roleServiceMock
            .Setup(s => s.RoleExistsAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _userService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Role 'User' does not exist");
        _identityServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldSucceedAndAssignRole_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "good-token");

        _identityServiceMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "jan", request.Email, false, true));
        _identityServiceMock
            .Setup(s => s.ConfirmEmailAsync(userId, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _roleServiceMock
            .Setup(s => s.RoleExistsAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _identityServiceMock
            .Setup(s => s.AddToRoleAsync(userId, "User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(userId, "jan", request.Email, true, true));

        var result = await _userService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Email confirmed");
        _identityServiceMock.Verify(s => s.AddToRoleAsync(userId, "User", It.IsAny<CancellationToken>()), Times.Once);
    }
}
