using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Infrastructure.Identity;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Errors;
using Users.Infrastructure.Identity.Models;

namespace UserModule.Integration;

public class PasswordRecoveryFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task ForgotPassword_ShouldStayGeneric_ForUnknownEmail()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await userService.ForgotPasswordAsync(new ForgotPasswordRequest("ghost@example.com"));

        // No message payload anymore: the endpoint only answers success/failure,
        // and it must answer success either way to avoid account enumeration.
        result.IsSuccess.Should().BeTrue(result.Error.Description);
    }

    [Fact]
    public async Task ForgotPassword_ShouldStayGeneric_ForKnownEmail()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(userService, email, ValidPassword);

        var result = await userService.ForgotPasswordAsync(new ForgotPasswordRequest(email));

        // Ответ не должен раскрывать, зарегистрирован ли email — оба кейса успешны.
        result.IsSuccess.Should().BeTrue(result.Error.Description);
    }

    [Fact]
    public async Task ResetPassword_ShouldReplacePassword_WhenTokenIsValid()
    {
        const string email = "jan@example.com";
        const string newPassword = "BrandNew1";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var identityUser = await userManager.FindByEmailAsync(email);
        var token = await userManager.GeneratePasswordResetTokenAsync(identityUser!);

        var resetResult = await userService.ResetPasswordAsync(
            new ResetPasswordDto(email, token, newPassword));

        resetResult.IsSuccess.Should().BeTrue(resetResult.Error.Description);

        var oldPasswordAuth = await userService.AuthenticateAsync(new AuthRequest(email, ValidPassword));
        oldPasswordAuth.IsSuccess.Should().BeFalse();
        oldPasswordAuth.Error.Should().Be(IdentityErrors.InvalidCredentials);

        var newPasswordAuth = await userService.AuthenticateAsync(new AuthRequest(email, newPassword));
        newPasswordAuth.IsSuccess.Should().BeTrue(newPasswordAuth.Error.Description);
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenTokenIsInvalid()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(userService, email, ValidPassword);
        await ConfirmEmailAsync(scope, email);

        var result = await userService.ResetPasswordAsync(
            new ResetPasswordDto(email, "garbage-token", "BrandNew1"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.PasswordResetFailed);

        var originalPasswordAuth = await userService.AuthenticateAsync(new AuthRequest(email, ValidPassword));
        originalPasswordAuth.IsSuccess.Should().BeTrue(originalPasswordAuth.Error.Description,
            "пароль не должен меняться при невалидном токене");
    }

    [Fact]
    public async Task ResetPassword_ShouldFail_WhenEmailIsUnknown()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await userService.ResetPasswordAsync(
            new ResetPasswordDto("ghost@example.com", "any-token", "BrandNew1"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(UserErrors.EmailNotRegistered);
    }
}
