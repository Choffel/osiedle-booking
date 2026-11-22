using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Notifications.Application.Interfaces;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Abstractions.Services.Tokens;
using Users.Application.DTOs;
using Users.Application.Errors;
using Users.Application.Services;
using Users.Domain.Entity.User;

namespace UserModule.Application.Services;

public class RegisterTests
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
    private readonly Mock<INotificationInternalApi> _notificationInternalApiMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly AuthService _authService;

    public RegisterTests()
    {
        _configurationMock.Setup(c => c["Frontend:Domain"])
            .Returns("http://localhost:4200");
        _authService = CreateAuthService();
    }

    private AuthService CreateAuthService() => new(
        _identityCredentialsMock.Object,
        _identityAccountMock.Object,
        _identityEmailMock.Object,
        _loggerMock.Object,
        _roleServiceMock.Object,
        _tokenServiceMock.Object,
        _userContextMock.Object,
        _unitOfWorkMock.Object,
        _userRepositoryMock.Object,
        _jwtTokenGeneratorMock.Object,
        _notificationInternalApiMock.Object,
        _configurationMock.Object);

    [Fact]
    public async Task RegisterAsync_ShouldSucceed_WhenIdentityIsCreated()
    {
        var request = new RegisterUserRequest("Jan", "Kowalski", "jan@example.com", "Password1");
        var identityId = Guid.NewGuid();

        _identityAccountMock
            .Setup(s => s.CreateIdentityAsync(request.Email, request.Email, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserId>.Success(UserId.From(identityId)));
        _identityEmailMock
            .Setup(s => s.GenerateEmailConfirmationTokenAsync(UserId.From(identityId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("confirm-token"));

        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.Should().Be(identityId);
        _userRepositoryMock.Verify(r => r.Add(
            It.Is<User>(u =>
                u.Id.Value == identityId &&
                u.Name.FirstName == request.FirstName &&
                u.Name.LastName == request.LastName &&
                u.Email == request.Email)), Times.Once);
        _identityEmailMock.Verify(
            s => s.GenerateEmailConfirmationTokenAsync(UserId.From(identityId), It.IsAny<CancellationToken>()), Times.Once);
        _notificationInternalApiMock.Verify(
            n => n.SendVerificationLinkAsync(
                request.Email,
                "http://localhost:4200/Authentication/confirm-email?email=jan%40example.com&token=confirm-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenIdentityCreationFails()
    {
        var request = new RegisterUserRequest("Jan", "Kowalski", "jan@example.com", "Password1");

        _identityAccountMock
            .Setup(s => s.CreateIdentityAsync(request.Email, request.Email, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserId>.Failure(UserErrors.RegistrationFailed));

        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.RegistrationFailed);
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _identityEmailMock.Verify(
            s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
