namespace Users.Application.DTOs;

public record ResetPasswordDto(string Email, string Token, string? NewPassword);