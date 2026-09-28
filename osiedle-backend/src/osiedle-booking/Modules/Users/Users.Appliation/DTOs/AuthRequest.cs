namespace Users.Appliation.DTOs;

public record AuthRequest(
    string Email,
    string Password
);