namespace Users.Application.DTOs.Identity;

public sealed record IdentityUserDto(
    Guid Id,
    string Email,
    IReadOnlyCollection<string> Roles
);