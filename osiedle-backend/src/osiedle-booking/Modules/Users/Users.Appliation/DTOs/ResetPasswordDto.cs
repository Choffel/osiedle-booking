namespace Users.Appliation.DTOs;

public record ResetPasswordDto(string Email, string Token, string? NewPassword);