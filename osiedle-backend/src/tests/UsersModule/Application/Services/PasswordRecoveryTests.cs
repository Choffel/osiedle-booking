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

public class PasswordRecoveryTests
{
    private readonly Mock<IIdentityCredentialsService> _identityCredentialsMock = new();
    private readonly Mock<IIdentityAccountService> _identityAccountMock = new();
    private readonly Mock<IIdentityEmailService> _identityEmailMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly AuthService _authService;

    public PasswordRecoveryTests()
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

    private static IdentityUserDto KnownUser(Guid id, string email) =>
        new(id, "jan", email, true, true);

    [Fact]
    public async Task ForgotPassword_ShouldReturnGenericMessage_WhenUserDoesNotExist()
    {
        var request = new ForgotPasswordRequest("ghost@example.com");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Failure("User not found"));

        var result = await _authService.ForgotPasswordAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("If this email address exists in the system");
        _identityCredentialsMock.Verify(
            s => s.GeneratePasswordResetTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnSameGenericMessage_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var request = new ForgotPasswordRequest("jan@example.com");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(KnownUser(userId, request.Email)));
        _identityCredentialsMock
            .Setup(s => s.GeneratePasswordResetTokenAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("reset-token"));

        var result = await _authService.ForgotPasswordAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("If this email address exists in the system");
        _identityCredentialsMock.Verify(
            s => s.GeneratePasswordResetTokenAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenEmailIsNotRegistered()
    {
        var request = new ResetPasswordDto("ghost@example.com", "any-token", "NewPassword1");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Failure("User not found"));

        var result = await _authService.ResetPasswordAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("ghost@example.com - this email address is not registered");
        _identityCredentialsMock.Verify(
            s => s.ResetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenIdentityRejectsToken()
    {
        var userId = Guid.NewGuid();
        var request = new ResetPasswordDto("jan@example.com", "bad-token", "NewPassword1");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(KnownUser(userId, request.Email)));
        _identityCredentialsMock
            .Setup(s => s.ResetPasswordAsync(userId, request.Token, request.NewPassword!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Failure("Failed to reset password"));

        var result = await _authService.ResetPasswordAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to reset password");
    }

    [Fact]
    public async Task ResetPassword_ShouldSucceed_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var request = new ResetPasswordDto("jan@example.com", "good-token", "NewPassword1");

        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto?>.Success(KnownUser(userId, request.Email)));
        _identityCredentialsMock
            .Setup(s => s.ResetPasswordAsync(userId, request.Token, request.NewPassword!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUserDto>.Success(KnownUser(userId, request.Email)));

        var result = await _authService.ResetPasswordAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Password reset successfully");
        _identityCredentialsMock.Verify(
            s => s.ResetPasswordAsync(userId, request.Token, "NewPassword1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
