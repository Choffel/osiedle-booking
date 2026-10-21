using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Infrastructure.Data;

namespace UserModule.Integration;

public class AuthenticationFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task Authenticate_ShouldSucceed_WhenCredentialsAreValidAndEmailConfirmed()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userId = await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await userService.AuthenticateAsync(new AuthRequest(email, ValidPassword));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Value!.AccessToken.Split('.').Should().HaveCount(3);
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storedToken = await db.RefreshTokens.AsNoTracking()
            .SingleOrDefaultAsync(rt => rt.Token == result.Value.RefreshToken);
        storedToken.Should().NotBeNull();
        storedToken!.UserId.Should().Be(userId);
        storedToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public async Task Authenticate_ShouldFail_WhenEmailIsNotConfirmed()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        await RegisterUserAsync(userService, email, ValidPassword);

        var result = await userService.AuthenticateAsync(new AuthRequest(email, ValidPassword));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid Email or Password");
    }

    [Fact]
    public async Task Authenticate_ShouldFail_WhenEmailIsUnknown()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.AuthenticateAsync(
            new AuthRequest("ghost@example.com", ValidPassword));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid Email or Password");
    }

    [Fact]
    public async Task Authenticate_ShouldFail_WhenPasswordIsWrong()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await userService.AuthenticateAsync(new AuthRequest(email, "WrongPassword1"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid Email or Password");
    }
}
