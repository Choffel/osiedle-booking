using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.User.Value_objects;
using Users.Infrastructure.Data;
using Users.Infrastructure.Identity;

namespace UserModule.Integration;

public class RegistrationFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_ShouldCreateIdentityAndDomainUser()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var userId = await RegisterUserAsync(userService, email, ValidPassword);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        identityUser.Should().NotBeNull();
        identityUser!.Id.Should().Be(userId);
        identityUser.EmailConfirmed.Should().BeFalse();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == new UserId(userId));
        domainUser.Should().NotBeNull();
        domainUser!.DisplayName.Should().Be("Jan");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldFail_WithoutCreatingSecondDomainUser()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        await RegisterUserAsync(userService, email, ValidPassword);

        var duplicate = await userService.RegisterAsync(
            new RegisterUserRequest("Jan", "Kowalski", email, ValidPassword), CancellationToken.None);

        duplicate.IsSuccess.Should().BeFalse();
        duplicate.Error.Should().Contain("already taken");

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUsersCount = await db.Users.AsNoTracking().CountAsync();
        domainUsersCount.Should().Be(1, "only the first registration creates a domain user");
    }

    [Fact]
    public async Task Register_WeakPassword_ShouldFail_WithoutCreatingAnyUser()
    {
        const string email = "weak@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.RegisterAsync(
            new RegisterUserRequest("Jan", "Kowalski", email, "abc"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("at least one digit");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await userManager.FindByEmailAsync(email)).Should().BeNull();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Users.AsNoTracking().CountAsync()).Should().Be(0);
    }
}
