using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Infrastructure.Identity;

namespace UserModule.Integration;

public class ConfirmEmailFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task ConfirmEmail_ShouldConfirmAddressAndAssignUserRole()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(userService, email, ValidPassword);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser!);

        var result = await userService.ConfirmEmailAsync(new ConfirmEmailRequest(email, token));

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.Should().Be("Email confirmed");

        var confirmedUser = await userManager.FindByEmailAsync(email);
        confirmedUser!.EmailConfirmed.Should().BeTrue();
        (await userManager.IsInRoleAsync(confirmedUser, "User")).Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenTokenIsInvalid()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(userService, email, ValidPassword);

        var result = await userService.ConfirmEmailAsync(new ConfirmEmailRequest(email, "garbage-token"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailConfirmationFailed);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        user!.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenEmailIsUnknown()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await userService.ConfirmEmailAsync(
            new ConfirmEmailRequest("ghost@example.com", "any-token"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotRegistered);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldFail_WhenUserRoleIsMissing()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(userService, email, ValidPassword);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RoleIdentity>>();
        var userRole = await roleManager.FindByNameAsync("User");
        (await roleManager.DeleteAsync(userRole!)).Succeeded.Should().BeTrue();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser!);

        var result = await userService.ConfirmEmailAsync(new ConfirmEmailRequest(email, token));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.RoleNotFound);
    }
}
