namespace Notifications.Application.DTOs.User;

public record PasswordResetDto(
    string Email,
    string Token,
    string NewPassword
);