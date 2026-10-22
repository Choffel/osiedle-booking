using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs.Identity;

namespace Users.Application.Abstractions.Services;

public interface IIdentityService
{
    Task<Result<Guid>> CreateIdentityAsync(string email, string userName, string password, CancellationToken ct);
    Task UpdateUserNameAsync(Guid userId, string newName, CancellationToken ct);
    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken ct);
    Task<IdentityUserDto?> FindByEmailAsync(string email, CancellationToken ct);
    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct);
    Task<string> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken ct);
    Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct);
    Task<IdentityUserDto> AddToRoleAsync(Guid userId, string roleName, CancellationToken ct);
    Task<IdentityUserDto> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct);
}