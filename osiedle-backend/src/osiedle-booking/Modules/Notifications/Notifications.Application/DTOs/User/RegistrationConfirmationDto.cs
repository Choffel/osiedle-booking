namespace Notifications.Application.DTOs.User;

public record RegistrationConfirmationDto(
    string Email,
    string Token
);