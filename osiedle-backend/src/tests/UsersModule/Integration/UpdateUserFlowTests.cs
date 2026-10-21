using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.User.Value_objects;
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
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        UserContext.UserId = userId;
        UserContext.Email = email;

        var result = await userService.UpdateUserAsync(new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Value.Should().Be("User updated successfully");

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == new UserId(userId));
        domainUser.DisplayName.Should().Be("Nowa nazwa");
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
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateUser_ShouldFail_WhenDomainUserIsMissing()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var domainUser = await db.Users.SingleAsync(u => u.Id == new UserId(userId));
        db.Users.Remove(domainUser);
        await db.SaveChangesAsync();

        UserContext.UserId = userId;
        UserContext.Email = email;

        var result = await userService.UpdateUserAsync(new UpdateUserDto("Nowa nazwa"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }
}
