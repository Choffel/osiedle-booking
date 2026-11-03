using FluentAssertions;
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

public class IdentityEmailServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly IdentityEmailService _sut;

    public IdentityEmailServiceTests()
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

        _sut = new IdentityEmailService(_userManagerMock.Object);
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_ShouldReturnToken_WhenUserExists()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirm-token");

        var result = await _sut.GenerateEmailConfirmationTokenAsync(UserId.From(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        ((Result<string>)result).Value.Should().Be("confirm-token");
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_ShouldFail_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.GenerateEmailConfirmationTokenAsync(UserId.From(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.NotFound);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldSucceed_WhenTokenIsValid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "good-token"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ConfirmEmailAsync(UserId.From(user.Id), "good-token", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldFail_WhenTokenIsInvalid()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "bad-token"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var result = await _sut.ConfirmEmailAsync(UserId.From(user.Id), "bad-token", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.EmailConfirmationFailed);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldFail_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.ConfirmEmailAsync(UserId.From(Guid.NewGuid()), "token", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.NotFound);
        _userManagerMock.Verify(
            m => m.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }
}
