using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.DTOs;
using Users.Application.DTOs.Identity;

namespace Users.Application.Abstractions.Services.Identity;

public interface IIdentityAccountService
{
    Task<Result<RegisterResponse>> CreateIdentityAsync(string email, string userName, CancellationToken ct = default);
    Task<Result<IdentityUserDto>> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<IdentityUserDto>> FindByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task<Result> UpdateUserNameAsync(UserId userId, string newName, CancellationToken ct = default);
    Task<Result> UpdateUserEmailAsync(UserId userId, string newEmail, CancellationToken ct = default);
    Task<Result> VerifyEmailConfirmationTokenAsync(string email, string code, CancellationToken ct = default);
    Task<Result<BookingOtpResult>> GetOrCreateAndGenerateOtpAsync(string email, CancellationToken ct = default);
}