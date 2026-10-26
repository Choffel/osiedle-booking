using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs.Identity;

namespace Users.Application.Abstractions.Services;

public interface IIdentityCredentialsService
{
    Task<Result<string>> CheckPasswordAsync(Guid userId, string password, CancellationToken ct);
    Task<Result<string>> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken ct);
    Task<Result<IdentityUserDto>> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct);
}