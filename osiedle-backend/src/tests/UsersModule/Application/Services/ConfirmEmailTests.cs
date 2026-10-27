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

public class ConfirmEmailTests
{
    private readonly Mock<IIdentityCredentialsService> _identityCredentialsMock = new();
    private readonly Mock<IIdentityAccountService> _identityAccountMock = new();
    private readonly Mock<IIdentityEmailService> _identityEmailMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly AuthService _authService;

    public ConfirmEmailTests()
    {
        _authService = new AuthService(
            _identityCredentialsMock.Object,
            _identityAccountMock.Object,
            _identityEmailMock.Object,
            _userRepositoryMock.Object,
            Mock.Of<ILogger<AuthService>>(),
            _roleServiceMock.Object,
            _tokenServiceMock.Object,
            _userContextMock.Object);
    }

    private static IdentityUserDto KnownUser(Guid id) => new(id, "jan", "jan@example.com", false, true);

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenUserDoesNotExist()
    {
        var request = new ConfirmEmailRequest("ghost@example.com", "any-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Failure(UserErrors.NotFound));

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotRegistered);
        _identityEmailMock.Verify(
            s => s.ConfirmEmailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _roleServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenTokenIsInvalid()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "bad-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(KnownUser(userId)));
        _identityEmailMock
            .Setup(s => s.ConfirmEmailAsync(userId, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(UserErrors.EmailConfirmationFailed));

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailConfirmationFailed);
        _roleServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenUserRoleIsMissing()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "good-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(KnownUser(userId)));
        _identityEmailMock
            .Setup(s => s.ConfirmEmailAsync(userId, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("Email confirmed"));
        _roleServiceMock
            .Setup(s => s.RoleExistsAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.RoleNotFound);
        _roleServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldSucceedAndAssignRole_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "good-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(KnownUser(userId)));
        _identityEmailMock
            .Setup(s => s.ConfirmEmailAsync(userId, request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("Email confirmed"));
        _roleServiceMock
            .Setup(s => s.RoleExistsAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _roleServiceMock
            .Setup(s => s.AddToRoleAsync(userId, "User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(new IdentityUserDto(
                userId, "jan", "jan@example.com", true, true)));

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Email confirmed");
        _roleServiceMock.Verify(s => s.AddToRoleAsync(userId, "User", It.IsAny<CancellationToken>()), Times.Once);
    }
}
