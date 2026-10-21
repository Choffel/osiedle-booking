using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;
using Users.Application.Services;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.Value_objects;

namespace UserModule.Application.Services;

public class UpdateUserTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly UserService _userService;
    private readonly Guid _currentUserId = Guid.NewGuid();

    public UpdateUserTests()
    {
        _userContextMock.SetupGet(c => c.Email).Returns("jan@example.com");
        _userContextMock.SetupGet(c => c.UserId).Returns(_currentUserId);

        _userService = new UserService(
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            Mock.Of<ILogger<UserService>>(),
            _roleServiceMock.Object,
            _tokenServiceMock.Object,
            _userContextMock.Object);
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenIdentityUserNotFound()
    {
        _identityServiceMock
            .Setup(s => s.FindByEmailAsync("jan@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserDto?)null);

        var result = await _userService.UpdateUserAsync(new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenDomainUserNotFound()
    {
        _identityServiceMock
            .Setup(s => s.FindByEmailAsync("jan@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(_currentUserId, "jan", "jan@example.com", true, true));
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _userService.UpdateUserAsync(new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
        _userRepositoryMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateDisplayName_WhenUserExists()
    {
        var domainUser = User.Create(new UserId(_currentUserId), "Stara nazwa");

        _identityServiceMock
            .Setup(s => s.FindByEmailAsync("jan@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUserDto(_currentUserId, "jan", "jan@example.com", true, true));
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainUser);

        var result = await _userService.UpdateUserAsync(new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("User updated successfully");
        domainUser.DisplayName.Should().Be("Nowa nazwa");
        _userRepositoryMock.Verify(r => r.Update(It.Is<User>(u => u.DisplayName == "Nowa nazwa")), Times.Once);
    }
}
