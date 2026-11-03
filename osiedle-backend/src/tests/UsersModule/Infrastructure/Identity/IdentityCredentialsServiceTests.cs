using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Errors;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Identity.Models;
using Users.Infrastructure.Identity.Services;

namespace UserModule.Infrastructure.Identity;

public class IdentityCredentialsServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly IdentityCredentialsService _sut;

    public IdentityCredentialsServiceTests()
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

        _sut = new IdentityCredentialsService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldSucceed_WhenPasswordIsValid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(SignInResult.Success);

        var result = await _sut.CheckPasswordAsync(UserId.From(user.Id), "Password1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldFail_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.CheckPasswordAsync(UserId.From(userId), "Password1", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.NotFound);
        _signInManagerMock.Verify(
            m => m.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldFail_WhenAccountIsLockedOut()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(SignInResult.LockedOut);

        var result = await _sut.CheckPasswordAsync(UserId.From(user.Id), "Password1", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.LockedOut);
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldFail_WhenPasswordIsInvalid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _signInManagerMock
            .Setup(m => m.CheckPasswordSignInAsync(user, "WrongPassword", true))
            .ReturnsAsync(SignInResult.Failed);

        var result = await _sut.CheckPasswordAsync(UserId.From(user.Id), "WrongPassword", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_ShouldFail_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.GeneratePasswordResetTokenAsync(UserId.From(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.NotFound);
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_ShouldReturnToken_WhenUserExists()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        var result = await _sut.GeneratePasswordResetTokenAsync(UserId.From(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        ((Result<string>)result).Value.Should().Be("reset-token");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldFail_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.ResetPasswordAsync(UserId.From(Guid.NewGuid()), "token", "NewPassword1", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.NotFound);
        _userManagerMock.Verify(
            m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldFail_WhenIdentityRejectsToken()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "bad-token", "NewPassword1"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var result = await _sut.ResetPasswordAsync(UserId.From(user.Id), "bad-token", "NewPassword1", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.FromIdentityResult("Invalid token"));
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldSucceed_WhenTokenIsValid()
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

        var result = await _sut.ResetPasswordAsync(UserId.From(user.Id), "good-token", "NewPassword1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        _userManagerMock.Verify(
            m => m.ResetPasswordAsync(user, "good-token", "NewPassword1"), Times.Once);
    }
}
