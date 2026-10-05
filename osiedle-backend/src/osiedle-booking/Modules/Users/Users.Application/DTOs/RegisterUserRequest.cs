namespace Users.Application.DTOs;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);