using osiedle_booking.BuildingBlocks;

namespace Users.Application.Abstractions.Services;

public interface IIdentityEmailService
{
    Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken ct);
    Task<Result<string>> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct);
}