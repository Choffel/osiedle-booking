using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs.Identity;
using Users.Application.Errors;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Identity.Models;
using Users.Infrastructure.Identity.Services;

namespace UserModule.Infrastructure.Identity;

public class IdentityAccountServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly IdentityAccountService _sut;

    public IdentityAccountServiceTests()
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

        _sut = new IdentityAccountService(_userManagerMock.Object);
    }

    [Fact]
    public async Task CreateIdentityAsync_ShouldSucceed_WhenIdentityIsCreated()
    {
        var email = "jan@example.com";
        var password = "Password1";
        var userName = "jan";

        ApplicationUser? created = null;
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .Callback<ApplicationUser, string>((u, _) => created = u)
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.CreateIdentityAsync(email, userName, password, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.Value.Should().NotBeEmpty();
        created!.Email.Should().Be(email);
        created.UserName.Should().Be(userName);
        result.Value.Value.Should().Be(created.Id);
    }

    [Fact]
    public async Task CreateIdentityAsync_ShouldFail_WhenIdentityIsRejected()
    {
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email is already taken" }));

        var result = await _sut.CreateIdentityAsync("jan@example.com", "jan", "Password1", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.FromIdentityResult("Email is already taken"));
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
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var result = await _sut.FindByEmailAsync("jan@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be("jan@example.com");
        result.Value.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldFail_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("ghost@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.FindByEmailAsync("ghost@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.NotFound);
    }

    [Fact]
    public async Task UpdateUserNameAsync_ShouldSucceed_WhenIdentityAcceptsNewName()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateUserNameAsync(UserId.From(user.Id), "nowy-nick", CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        ((Result<string>)result).Value.Should().Be("nowy-nick");
        user.UserName.Should().Be("nowy-nick");
    }

    [Fact]
    public async Task UpdateUserNameAsync_ShouldFail_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.UpdateUserNameAsync(UserId.From(Guid.NewGuid()), "nowy-nick", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.NotFound);
    }

    [Fact]
    public async Task UpdateUserNameAsync_ShouldFail_WhenIdentityRejectsNewName()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jan" };
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Usernames cannot contain spaces." }));

        var result = await _sut.UpdateUserNameAsync(UserId.From(user.Id), "zla nazwa", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.FromIdentityResult("Usernames cannot contain spaces."));
    }
}
