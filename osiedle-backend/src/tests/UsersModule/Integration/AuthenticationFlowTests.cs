using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Infrastructure.Data;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Errors;

namespace UserModule.Integration;

public class AuthenticationFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task Authenticate_ShouldSucceed_WhenCredentialsAreValidAndEmailConfirmed()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userId = await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await authService.AuthenticateAsync(new AuthRequest(email, ValidPassword));

        result.IsSuccess.Should().BeTrue(result.Error.Description);
        var tokens = result.Value;
        tokens.AccessToken.Split('.').Should().HaveCount(3);
        tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storedToken = await db.RefreshTokens.AsNoTracking()
            .SingleOrDefaultAsync(rt => rt.Token == tokens.RefreshToken);
        storedToken.Should().NotBeNull();
        storedToken!.UserId.Should().Be(UserId.From(userId));
        storedToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Authenticate_ShouldFail_WhenEmailIsNotConfirmed()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(authService, email, ValidPassword);

        var result = await authService.AuthenticateAsync(new AuthRequest(email, ValidPassword));

        // RequireConfirmedEmail makes SignInManager reject the sign-in, and the
        // credentials service maps that to the generic invalid-credentials error.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Authenticate_ShouldFail_WhenEmailIsUnknown()
    {
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await authService.AuthenticateAsync(
            new AuthRequest("ghost@example.com", ValidPassword));

        // Unknown email and wrong password must return the same error — no account enumeration.
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Authenticate_ShouldFail_WhenPasswordIsWrong()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(authService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await authService.AuthenticateAsync(new AuthRequest(email, "WrongPassword1"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
    }
}
