using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs;

namespace Users.Application.Abstractions.Services.Identity;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterUserRequest request, CancellationToken ct);

    Task<Result<TokenResponseDto>> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);
    Task<Result<string>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
}