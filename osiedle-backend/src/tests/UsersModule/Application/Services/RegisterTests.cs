using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
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
    private readonly AuthService _authService;

    public RegisterTests()
    {
        _authService = CreateAuthService();
    }

    private AuthService CreateAuthService() => new(
        _identityCredentialsMock.Object,
        _identityAccountMock.Object,
        _identityEmailMock.Object,
        _userRepositoryMock.Object,
        _loggerMock.Object,
        _roleServiceMock.Object,
        _tokenServiceMock.Object,
        _userContextMock.Object);

    [Fact]
    public async Task RegisterAsync_ShouldSucceed_WhenIdentityIsCreated()
    {
        var request = new RegisterUserRequest("Jan", "Kowalski", "jan@example.com", "Password1");
        var identityId = Guid.NewGuid();

        _identityAccountMock
            .Setup(s => s.CreateIdentityAsync(request.Email, request.FirstName, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(identityId));
        _identityEmailMock
            .Setup(s => s.GenerateEmailConfirmationTokenAsync(identityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("confirm-token"));

        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(identityId);
        _userRepositoryMock.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Id.Value == identityId && u.DisplayName == request.FirstName),
            It.IsAny<CancellationToken>()), Times.Once);
        _identityEmailMock.Verify(
            s => s.GenerateEmailConfirmationTokenAsync(identityId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenIdentityCreationFails()
    {
        var request = new RegisterUserRequest("Jan", "Kowalski", "jan@example.com", "Password1");

        _identityAccountMock
            .Setup(s => s.CreateIdentityAsync(request.Email, request.FirstName, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure(UserErrors.RegistrationFailed));

        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.RegistrationFailed);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityEmailMock.Verify(
            s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
