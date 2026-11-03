using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Infrastructure.Data;
using Users.Application.Abstractions.Services.Identity;
using Users.Infrastructure.Identity.Models;

namespace UserModule.Integration;

public class UpdateUserFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateUserName_PersistsDomainAndIdentityName()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await userService.UpdateUserNameAsync(
            UserId.From(userId), email, new UpdateUserDto("Nowanazwa", "Kowalski"));

        result.IsSuccess.Should().BeTrue(result.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == UserId.From(userId));
        domainUser.Name.FirstName.Should().Be("Nowanazwa");
        domainUser.Name.LastName.Should().Be("Kowalski");

        var identityUser = await db.Set<ApplicationUser>().AsNoTracking().SingleAsync(u => u.Id == userId);
        identityUser.UserName.Should().Be("Nowanazwa");
    }

    [Fact]
    public async Task UpdateUserName_UnknownEmail_ReturnsNotFound()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.UpdateUserNameAsync(
            UserId.From(Guid.NewGuid()), "ghost@example.com", new UpdateUserDto("Nowa", "Nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task UpdateUserName_DomainUserMissing_ReturnsNotFound()
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

        var result = await userService.UpdateUserNameAsync(
            UserId.From(userId), email, new UpdateUserDto("Nowa", "Nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task UpdateUserEmail_PersistsDomainAndIdentityEmail()
    {
        const string email = "jan@example.com";
        const string newEmail = "nowy-email@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await userService.UpdateUserEmailAsyc(UserId.From(userId), newEmail);

        result.IsSuccess.Should().BeTrue(result.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == UserId.From(userId));
        domainUser.Email.Should().Be(newEmail);

        var identityUser = await db.Set<ApplicationUser>().AsNoTracking().SingleAsync(u => u.Id == userId);
        identityUser.Email.Should().Be(newEmail);
    }

    [Fact]
    public async Task UpdateUserEmail_UnknownUser_ReturnsNotFound()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.UpdateUserEmailAsyc(UserId.From(Guid.NewGuid()), "new@example.com");

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
        var profile = result.Value;
        profile.FirstName.Should().Be("Jan");
        profile.LastName.Should().Be("Kowalski");
    }
}
