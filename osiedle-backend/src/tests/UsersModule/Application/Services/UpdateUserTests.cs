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

    private static IdentityUserDto IdentityUser(Guid id) =>
        new(id, $"user-{id:N}@example.com", new[] { "User" });

    private void SetupEmailLookup(bool found) =>
        _identityAccountMock
            .Setup(s => s.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(found
                ? Result<IdentityUserDto>.Success(IdentityUser(_currentUserId))
                : Result<IdentityUserDto>.Failure(IdentityErrors.NotFound));

    private void SetupIdLookup(bool found) =>
        _identityAccountMock
            .Setup(s => s.FindByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(found
                ? Result<IdentityUserDto>.Success(IdentityUser(_currentUserId))
                : Result<IdentityUserDto>.Failure(IdentityErrors.NotFound));

    private void SetupDomainUser(User user) =>
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    private User NewDomainUser(string first = "Stara", string last = "Nazwa") =>
        User.Create(UserId.From(_currentUserId), Name.Create(first, last), $"user-{_currentUserId:N}@example.com");

    // ---------- UpdateUserNameAsync ----------

    [Fact]
    public async Task UpdateUserName_UnknownEmail_ReturnsNotFound()
    {
        SetupEmailLookup(found: false);

        var result = await _userService.UpdateUserNameAsync(
            UserId.From(_currentUserId), "ghost@example.com", new UpdateUserDto("Nowa", "Nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserName_DomainUserMissing_ReturnsNotFound()
    {
        SetupEmailLookup(found: true);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _userService.UpdateUserNameAsync(
            UserId.From(_currentUserId), "any@example.com", new UpdateUserDto("Nowa", "Nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
        _identityAccountMock.Verify(
            s => s.UpdateUserNameAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserName_PersistsDomainAndIdentityName()
    {
        var domainUser = NewDomainUser();
        SetupEmailLookup(found: true);
        SetupDomainUser(domainUser);
        _identityAccountMock
            .Setup(s => s.UpdateUserNameAsync(UserId.From(_currentUserId), "Nowa", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _userService.UpdateUserNameAsync(
            UserId.From(_currentUserId), "any@example.com", new UpdateUserDto("Nowa", "Nazwa"));

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        domainUser.Name.FirstName.Should().Be("Nowa");
        domainUser.Name.LastName.Should().Be("Nazwa");
        _identityAccountMock.Verify(
            s => s.UpdateUserNameAsync(UserId.From(_currentUserId), "Nowa", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserName_IdentityRejects_ReturnsUpdateFailed()
    {
        var domainUser = NewDomainUser();
        SetupEmailLookup(found: true);
        SetupDomainUser(domainUser);
        _identityAccountMock
            .Setup(s => s.UpdateUserNameAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(IdentityErrors.FromIdentityResult("Usernames cannot contain spaces.")));

        var result = await _userService.UpdateUserNameAsync(
            UserId.From(_currentUserId), "any@example.com", new UpdateUserDto("zla nazwa", "Nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.UpdateFailed);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- UpdateUserEmailAsyc ----------

    [Fact]
    public async Task UpdateUserEmail_UnknownUser_ReturnsNotFound()
    {
        SetupIdLookup(found: false);

        var result = await _userService.UpdateUserEmailAsyc(
            UserId.From(_currentUserId), "new@example.com");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserEmail_DomainUserMissing_ReturnsNotFound()
    {
        SetupIdLookup(found: true);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<UserId>(id => id.Value == _currentUserId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _userService.UpdateUserEmailAsyc(
            UserId.From(_currentUserId), "new@example.com");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
        _identityAccountMock.Verify(
            s => s.UpdateUserEmailAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserEmail_PersistsDomainAndIdentityEmail()
    {
        var domainUser = NewDomainUser();
        SetupIdLookup(found: true);
        SetupDomainUser(domainUser);
        _identityAccountMock
            .Setup(s => s.UpdateUserEmailAsync(UserId.From(_currentUserId), "new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _userService.UpdateUserEmailAsyc(
            UserId.From(_currentUserId), "new@example.com");

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        domainUser.Email.Should().Be("new@example.com");
        _identityAccountMock.Verify(
            s => s.UpdateUserEmailAsync(UserId.From(_currentUserId), "new@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserEmail_IdentityRejects_ReturnsUpdateFailed()
    {
        var domainUser = NewDomainUser();
        SetupIdLookup(found: true);
        SetupDomainUser(domainUser);
        _identityAccountMock
            .Setup(s => s.UpdateUserEmailAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(IdentityErrors.FromIdentityResult("Invalid email.")));

        var result = await _userService.UpdateUserEmailAsyc(
            UserId.From(_currentUserId), "broken@example.com");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.UpdateFailed);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- GetUserByIdAsync ----------

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
        var profile = result.Value;
        profile.FirstName.Should().Be("Jan");
        profile.LastName.Should().Be("Kowalski");
    }
}
