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
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly UserService _userService;

    public RegisterTests()
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
    public async Task RegisterAsync_ShouldSucceed_WhenIdentityIsCreated()
    {
        var request = new RegisterUserRequest("Jan", "Kowalski", "jan@example.com", "Password1");
        var identityId = Guid.NewGuid();

        _identityServiceMock
            .Setup(s => s.CreateIdentityAsync(request.Email, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(identityId));
        _identityServiceMock
            .Setup(s => s.GenerateEmailConfirmationTokenAsync(identityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("confirm-token");

        var result = await _userService.RegisterAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(identityId);
        _userRepositoryMock.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Id.Value == identityId && u.DisplayName == request.FirstName),
            It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceMock.Verify(
            s => s.GenerateEmailConfirmationTokenAsync(identityId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenIdentityCreationFails()
    {
        var request = new RegisterUserRequest("Jan", "Kowalski", "jan@example.com", "Password1");

        _identityServiceMock
            .Setup(s => s.CreateIdentityAsync(request.Email, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure("Email is already taken"));

        var result = await _userService.RegisterAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already taken");
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceMock.Verify(
            s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
