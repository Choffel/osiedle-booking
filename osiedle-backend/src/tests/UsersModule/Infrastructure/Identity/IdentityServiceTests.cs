using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs.Identity;
using Users.Infrastructure.Identity;

namespace UserModule.Infrastructure.Identity;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly IdentityService _sut;

    public IdentityServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        _sut = new IdentityService(
            _userManagerMock.Object,
            Mock.Of<ILogger<IdentityService>>(),
            _signInManagerMock.Object);
    }

    [Fact]
    public async Task CreateIdentityAsync_ShouldSucceed_WhenIdentityIsCreated()
    {
        var email = "jan@example.com";
        var password = "Password1";

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.CreateIdentityAsync(email, password, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _userManagerMock.Verify(m => m.CreateAsync(
            It.Is<ApplicationUser>(u => u.Email == email && u.UserName == email), password), Times.Once);
    }

    [Fact]
    public async Task CreateIdentityAsync_ShouldFail_WhenIdentityIsRejected()
    {
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email is already taken" }));

        var result = await _sut.CreateIdentityAsync("jan@example.com", "Password1", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already taken");
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnDto_WhenUserExists()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "jan",
            Email = "jan@example.com",
            EmailConfirmed = true
        };
        _userManagerMock.Setup(m => m.FindByEmailAsync("jan@example.com")).ReturnsAsync(user);

        var dto = await _sut.FindByEmailAsync("jan@example.com", CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(user.Id);
        dto.UserName.Should().Be("jan");
        dto.Email.Should().Be("jan@example.com");
        dto.EmailConfirmed.Should().BeTrue();
        dto.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("ghost@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var dto = await _sut.FindByEmailAsync("ghost@example.com", CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.CheckPasswordAsync(userId, "Password1", CancellationToken.None);

        result.Should().BeFalse();
        _signInManagerMock.Verify(
            m => m.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnTrue_WhenPasswordIsValid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(SignInResult.Success);

        var result = await _sut.CheckPasswordAsync(user.Id, "Password1", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnFalse_WhenUserIsLockedOut()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(SignInResult.LockedOut);

        var result = await _sut.CheckPasswordAsync(user.Id, "Password1", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnFalse_WhenPasswordIsInvalid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.CheckPasswordSignInAsync(user, "WrongPassword", true))
            .ReturnsAsync(SignInResult.Failed);

        var result = await _sut.CheckPasswordAsync(user.Id, "WrongPassword", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.ConfirmEmailAsync(Guid.NewGuid(), "token", CancellationToken.None);

        result.Should().BeFalse();
        _userManagerMock.Verify(
            m => m.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnTrue_WhenTokenIsValid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "good-token"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ConfirmEmailAsync(user.Id, "good-token", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnFalse_WhenTokenIsInvalid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "bad-token"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var result = await _sut.ConfirmEmailAsync(user.Id, "bad-token", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await FluentActions
            .Awaiting(() => _sut.GenerateEmailConfirmationTokenAsync(Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await FluentActions
            .Awaiting(() => _sut.GeneratePasswordResetTokenAsync(Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnFailedDto_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var dto = await _sut.AddToRoleAsync(Guid.NewGuid(), "User", CancellationToken.None);

        dto.Succeeded.Should().BeFalse();
        dto.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnFailedDto_WhenAddingToRoleFails()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.AddToRoleAsync(user, "GhostRole"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role does not exist" }));

        var dto = await _sut.AddToRoleAsync(user.Id, "GhostRole", CancellationToken.None);

        dto.Succeeded.Should().BeFalse();
        dto.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnDto_WhenRoleIsAdded()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "jan",
            Email = "jan@example.com",
            EmailConfirmed = true
        };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.AddToRoleAsync(user, "User"))
            .ReturnsAsync(IdentityResult.Success);

        var dto = await _sut.AddToRoleAsync(user.Id, "User", CancellationToken.None);

        dto.Succeeded.Should().BeTrue();
        dto.Id.Should().Be(user.Id);
        dto.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFailedDto_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var dto = await _sut.ResetPasswordAsync(Guid.NewGuid(), "token", "NewPassword1", CancellationToken.None);

        dto.Succeeded.Should().BeFalse();
        dto.Id.Should().Be(Guid.Empty);
        _userManagerMock.Verify(
            m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFailedDto_WhenIdentityRejectsToken()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "bad-token", "NewPassword1"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var dto = await _sut.ResetPasswordAsync(user.Id, "bad-token", "NewPassword1", CancellationToken.None);

        dto.Succeeded.Should().BeFalse();
        dto.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnDto_WhenResetSucceeds()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "jan",
            Email = "jan@example.com"
        };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "good-token", "NewPassword1"))
            .ReturnsAsync(IdentityResult.Success);

        var dto = await _sut.ResetPasswordAsync(user.Id, "good-token", "NewPassword1", CancellationToken.None);

        dto.Succeeded.Should().BeTrue();
        dto.Id.Should().Be(user.Id);
    }
}
