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

public class PasswordRecoveryTests
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

    public PasswordRecoveryTests()
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

    private static IdentityUserDto KnownUser(Guid id, string email) =>
        new(id, email, new[] { "User" });

    [Fact]
    public async Task ForgotPassword_ShouldStayGeneric_WhenUserDoesNotExist()
    {
        var request = new ForgotPasswordRequest("ghost@example.com");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Failure(IdentityErrors.NotFound));

        var result = await _authService.ForgotPasswordAsync(request);

        // The endpoint must answer success regardless of whether the email exists.
        result.IsSuccess.Should().BeTrue(result.Error.Description);
        _identityCredentialsMock.Verify(
            s => s.GeneratePasswordResetTokenAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_ShouldStayGeneric_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var request = new ForgotPasswordRequest("jan@example.com");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(KnownUser(userId, request.Email)));
        _identityCredentialsMock
            .Setup(s => s.GeneratePasswordResetTokenAsync(UserId.From(userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("reset-token"));

        var result = await _authService.ForgotPasswordAsync(request);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        _identityCredentialsMock.Verify(
            s => s.GeneratePasswordResetTokenAsync(UserId.From(userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenEmailIsNotRegistered()
    {
        var request = new ResetPasswordDto("ghost@example.com", "any-token", "NewPassword1");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Failure(IdentityErrors.NotFound));

        var result = await _authService.ResetPasswordAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotRegistered);
        _identityCredentialsMock.Verify(
            s => s.ResetPasswordAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenIdentityRejectsToken()
    {
        var userId = Guid.NewGuid();
        var request = new ResetPasswordDto("jan@example.com", "bad-token", "NewPassword1");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(KnownUser(userId, request.Email)));
        _identityCredentialsMock
            .Setup(s => s.ResetPasswordAsync(UserId.From(userId), request.Token, request.NewPassword!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(IdentityErrors.FromIdentityResult("Invalid token.")));

        var result = await _authService.ResetPasswordAsync(request);

        // AuthService maps any credentials failure to a single domain error.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.PasswordResetFailed);
    }

    [Fact]
    public async Task ResetPassword_ShouldSucceed_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var request = new ResetPasswordDto("jan@example.com", "good-token", "NewPassword1");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(KnownUser(userId, request.Email)));
        _identityCredentialsMock
            .Setup(s => s.ResetPasswordAsync(UserId.From(userId), request.Token, request.NewPassword!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _authService.ResetPasswordAsync(request);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        _identityCredentialsMock.Verify(
            s => s.ResetPasswordAsync(UserId.From(userId), request.Token, "NewPassword1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
