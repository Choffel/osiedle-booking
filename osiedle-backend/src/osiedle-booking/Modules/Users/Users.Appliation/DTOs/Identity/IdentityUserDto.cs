namespace Users.Appliation.DTOs.Identity;

public sealed record IdentityUserDto(
    Guid Id,
    string UserName,
    string Email,
    bool EmailConfirmed,
    bool Succeeded
);