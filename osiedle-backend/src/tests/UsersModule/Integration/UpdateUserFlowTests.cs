using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Infrastructure.Data;

namespace UserModule.Integration;

public class UpdateUserFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateUser_ShouldPersistNewDisplayName()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        UserContext.UserId = userId;
        UserContext.Email = email;

        var result = await userService.UpdateUserAsync(new UpdateUserDto("Nowanazwa"));

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        result.Value.Should().Be("User updated successfully");

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == new UserId(userId));
        domainUser.DisplayName.Should().Be("Nowanazwa");
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenContextEmailIsUnknown()
    {
        UserContext.UserId = Guid.NewGuid();
        UserContext.Email = "ghost@example.com";

        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.UpdateUserAsync(new UpdateUserDto("Nowa nazwa"));

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
        var domainUser = await db.Users.SingleAsync(u => u.Id == new UserId(userId));
        db.Users.Remove(domainUser);
        await db.SaveChangesAsync();

        UserContext.UserId = userId;
        UserContext.Email = email;

        var result = await userService.UpdateUserAsync(new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.NotFound);
    }
}
