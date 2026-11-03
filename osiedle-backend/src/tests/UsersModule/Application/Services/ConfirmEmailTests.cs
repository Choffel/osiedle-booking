using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.Roles;
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

public class ConfirmEmailTests
{
    private readonly Mock<IIdentityCredentialsService> _identityCredentialsMock = new();
    private readonly Mock<IIdentityAccountService> _identityAccountMock = new();
    private readonly Mock<IIdentityEmailService> _identityEmailMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthService _authService;

    public ConfirmEmailTests()
    {
        _authService = new AuthService(
            _identityCredentialsMock.Object,
            _identityAccountMock.Object,
            _identityEmailMock.Object,
            Mock.Of<ILogger<AuthService>>(),
            _roleServiceMock.Object,
            _tokenServiceMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    private static IdentityUserDto KnownUser(Guid id) =>
        new(id, "jan@example.com", new[] { "User" });

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenUserDoesNotExist()
    {
        var request = new ConfirmEmailRequest("ghost@example.com", "any-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Failure(IdentityErrors.NotFound));

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotRegistered);
        _identityEmailMock.Verify(
            s => s.ConfirmEmailAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _roleServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenTokenIsInvalid()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "bad-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(KnownUser(userId)));
        _identityEmailMock
            .Setup(s => s.ConfirmEmailAsync(UserId.From(userId), request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(IdentityErrors.EmailConfirmationFailed));

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailConfirmationFailed);
        _roleServiceMock.Verify(
            s => s.AddToRoleAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenRoleAssignmentFails()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "good-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(KnownUser(userId)));
        _identityEmailMock
            .Setup(s => s.ConfirmEmailAsync(UserId.From(userId), request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _roleServiceMock
            .Setup(s => s.AddToRoleAsync(UserId.From(userId), UserRoles.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(RoleErrors.AddUserToRoleFailed));

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(RoleErrors.AddUserToRoleFailed);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldSucceedAndAssignUserRole_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var request = new ConfirmEmailRequest("jan@example.com", "good-token");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(KnownUser(userId)));
        _identityEmailMock
            .Setup(s => s.ConfirmEmailAsync(UserId.From(userId), request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _roleServiceMock
            .Setup(s => s.AddToRoleAsync(UserId.From(userId), UserRoles.User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _authService.ConfirmEmailAsync(request);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        _roleServiceMock.Verify(
            s => s.AddToRoleAsync(UserId.From(userId), UserRoles.User, It.IsAny<CancellationToken>()), Times.Once);
    }
}
