using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Infrastructure.Identity;

namespace UserModule.Integration;

public class PasswordRecoveryFlowTests : IntegrationTestBase
{
    private const string GenericForgotPasswordMessage =
        "If this email address exists in the system, instructions have been sent to the email.";

    [Fact]
    public async Task ForgotPassword_ShouldReturnGenericMessage_ForUnknownEmail()
    {
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await userService.ForgotPasswordAsync(new ForgotPasswordRequest("ghost@example.com"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(GenericForgotPasswordMessage);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnSameGenericMessage_ForKnownEmail()
    {
        const string email = "jan@example.com";
        using var scope = CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await RegisterUserAsync(userService, email, ValidPassword);

        var result = await userService.ForgotPasswordAsync(new ForgotPasswordRequest(email));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(GenericForgotPasswordMessage,
            "ответ не должен раскрывать, зарегистрирован ли email");
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

        resetResult.IsSuccess.Should().BeTrue(resetResult.Error);
        resetResult.Value.Should().Be("Password reset successfully");

        var oldPasswordAuth = await userService.AuthenticateAsync(new AuthRequest(email, ValidPassword));
        oldPasswordAuth.IsSuccess.Should().BeFalse();
        oldPasswordAuth.Error.Should().Be("Invalid Email or Password");

        var newPasswordAuth = await userService.AuthenticateAsync(new AuthRequest(email, newPassword));
        newPasswordAuth.IsSuccess.Should().BeTrue(newPasswordAuth.Error);
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
        result.Error.Should().Be("Failed to reset password");

        var originalPasswordAuth = await userService.AuthenticateAsync(new AuthRequest(email, ValidPassword));
        originalPasswordAuth.IsSuccess.Should().BeTrue(originalPasswordAuth.Error,
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
        result.Error.Should().Be("ghost@example.com - this email address is not registered");
    }
}
