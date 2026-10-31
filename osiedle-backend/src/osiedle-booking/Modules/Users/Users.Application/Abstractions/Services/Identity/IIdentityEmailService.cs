using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Users.Application.Abstractions.Services.Identity;

public interface IIdentityEmailService
{
    Task<Result> GenerateEmailConfirmationTokenAsync(UserId userId, CancellationToken ct);
    Task<Result> ConfirmEmailAsync(UserId userId, string token, CancellationToken ct);
}