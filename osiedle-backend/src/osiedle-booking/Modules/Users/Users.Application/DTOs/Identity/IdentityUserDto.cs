namespace Users.Application.DTOs.Identity;

public sealed record IdentityUserDto(
    Guid Id,
    string UserName,
    string Email,
    bool EmailConfirmed,
    bool Succeeded
);