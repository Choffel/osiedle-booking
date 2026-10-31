using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs.Identity;

namespace Users.Application.Abstractions.Services.Identity;

public interface IIdentityCredentialsService
{
    Task<Result> CheckPasswordAsync(UserId userId, string password, CancellationToken ct);
    Task<Result> GeneratePasswordResetTokenAsync(UserId userId, CancellationToken ct);
    Task<Result> ResetPasswordAsync(UserId userId, string token, string newPassword, CancellationToken ct);
}