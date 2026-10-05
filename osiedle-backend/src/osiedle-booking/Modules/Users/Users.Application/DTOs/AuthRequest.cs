namespace Users.Application.DTOs;

public record AuthRequest(
    string Email,
    string Password
);