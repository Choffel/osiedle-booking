using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Infrastructure.Data;
using Users.Application.Abstractions.Services.Identity;
using Users.Infrastructure.Identity.Models;

namespace UserModule.Integration;

public class RegistrationFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_ShouldCreateIdentityAndDomainUser()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var userId = await RegisterUserAsync(authService, email, ValidPassword);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        identityUser.Should().NotBeNull();
        identityUser!.Id.Should().Be(userId);
        identityUser.EmailConfirmed.Should().BeFalse();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == UserId.From(userId));
        domainUser.Should().NotBeNull();
        domainUser!.Name.FirstName.Should().Be("Jan");
        domainUser.Name.LastName.Should().Be("Kowalski");
        domainUser.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldFail_WithoutCreatingSecondDomainUser()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(authService, email, ValidPassword);

        var duplicate = await authService.RegisterAsync(
            new RegisterUserRequest("Jan", "Kowalski", email, ValidPassword), CancellationToken.None);

        // Identity errors are passed through as IdentityError-coded failures whose
        // description contains the raw Identity message.
        duplicate.IsSuccess.Should().BeFalse();
        duplicate.Error.Code.Should().Be("IdentityError");

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUsersCount = await db.Users.AsNoTracking().CountAsync();
        domainUsersCount.Should().Be(1, "only the first registration creates a domain user");
    }

    [Fact]
    public async Task Register_WeakPassword_ShouldFail_WithoutCreatingAnyUser()
    {
        const string email = "weak@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await authService.RegisterAsync(
            new RegisterUserRequest("Jan", "Kowalski", email, "abc"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("IdentityError");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await userManager.FindByEmailAsync(email)).Should().BeNull();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Users.AsNoTracking().CountAsync()).Should().Be(0);
    }
}
