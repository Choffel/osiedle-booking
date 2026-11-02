using FluentAssertions;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;
using Users.Application.Errors;
using Users.Application.Services;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;

namespace UserModule.Application.Services;

public class UpdateUserTests
{
    private readonly Mock<IIdentityAccountService> _identityAccountMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserReadRepository> _userReadRepositoryMock = new();
    private readonly UserService _userService;
    private readonly Guid _currentUserId = Guid.NewGuid();

    public UpdateUserTests()
    {
        _userService = new UserService(
            _identityAccountMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userReadRepositoryMock.Object);
    }

    private void SetupIdentityLookup(bool found) =>
        _identityAccountMock
            .Setup(s => s.FindByEmailAsync("jan@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(found
                ? Result<IdentityUserDto>.Success(
                    new IdentityUserDto(_currentUserId, "jan@example.com", new[] { "User" }))
                : Result<IdentityUserDto>.Failure(IdentityErrors.NotFound));

    private void SetupDomainUser(User user) =>
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenIdentityUserNotFound()
    {
        SetupIdentityLookup(found: false);

        var result = await _userService.UpdateUserAsync(
            UserId.From(_currentUserId), "jan@example.com", new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityAccountMock.Verify(
            s => s.UpdateUserNameAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenDomainUserNotFound()
    {
        SetupIdentityLookup(found: true);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _userService.UpdateUserAsync(
            UserId.From(_currentUserId), "jan@example.com", new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
        _identityAccountMock.Verify(
            s => s.UpdateUserNameAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ShouldPersistNewUserName_WhenUserExists()
    {
        var domainUser = User.Create(
            UserId.From(_currentUserId), Name.Create("Stara", "Nazwa"), "jan@example.com");

        SetupIdentityLookup(found: true);
        SetupDomainUser(domainUser);
        _identityAccountMock
            .Setup(s => s.UpdateUserNameAsync(UserId.From(_currentUserId), "Nowanazwa", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _userService.UpdateUserAsync(
            UserId.From(_currentUserId), "jan@example.com", new UpdateUserDto("Nowanazwa"));

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        _identityAccountMock.Verify(
            s => s.UpdateUserNameAsync(UserId.From(_currentUserId), "Nowanazwa", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // A username change only touches the identity account, the domain profile stays.
        domainUser.Name.FirstName.Should().Be("Stara");
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenIdentityRejectsNewUserName()
    {
        var domainUser = User.Create(
            UserId.From(_currentUserId), Name.Create("Stara", "Nazwa"), "jan@example.com");

        SetupIdentityLookup(found: true);
        SetupDomainUser(domainUser);
        _identityAccountMock
            .Setup(s => s.UpdateUserNameAsync(UserId.From(_currentUserId), "zla nazwa", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(IdentityErrors.FromIdentityResult("Usernames cannot contain spaces.")));

        var result = await _userService.UpdateUserAsync(
            UserId.From(_currentUserId), "jan@example.com", new UpdateUserDto("zla nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.UpdateFailed);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserById_ShouldFail_WhenProfileIsMissing()
    {
        _userReadRepositoryMock
            .Setup(r => r.GetProfileAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        var result = await _userService.GetUserByIdAsync(UserId.From(_currentUserId));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnProfile_WhenItExists()
    {
        _userReadRepositoryMock
            .Setup(r => r.GetProfileAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDto(_currentUserId, "Jan", "Kowalski", "jan@example.com"));

        var result = await _userService.GetUserByIdAsync(UserId.From(_currentUserId));

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        var profile = ((Result<CurrentUserDto>)result).Value;
        profile.FirstName.Should().Be("Jan");
        profile.LastName.Should().Be("Kowalski");
    }
}
