using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Infrastructure.Data;
using Users.Infrastructure.Identity.Models;
using Users.Application.Abstractions.Services.Identity;

namespace UserModule.Integration;

public class UpdateUserFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateUser_ShouldPersistNewUserName()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await userService.UpdateUserAsync(
            UserId.From(userId), email, new UpdateUserDto("Nowanazwa"));

        result.IsSuccess.Should().BeTrue(result.Error.Description);

        // The dto UserName is persisted as the identity username.
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        identityUser!.UserName.Should().Be("Nowanazwa");

        // The domain profile name stays untouched by a username change.
        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == UserId.From(userId));
        domainUser.Name.FirstName.Should().Be("Jan");
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenIdentityEmailIsUnknown()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.UpdateUserAsync(
            UserId.From(Guid.NewGuid()), "ghost@example.com", new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenDomainUserIsMissing()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.SingleAsync(u => u.Id == UserId.From(userId));
        db.Users.Remove(domainUser);
        await db.SaveChangesAsync();

        var result = await userService.UpdateUserAsync(
            UserId.From(userId), email, new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnCurrentUser_WhenProfileExists()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await userService.GetUserByIdAsync(UserId.From(userId));

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        var profile = ((osiedle_booking.BuildingBlocks.Result<CurrentUserDto>)result).Value;
        profile.FirstName.Should().Be("Jan");
        profile.LastName.Should().Be("Kowalski");
    }
}
