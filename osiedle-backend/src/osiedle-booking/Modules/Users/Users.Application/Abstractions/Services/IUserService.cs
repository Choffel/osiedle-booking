using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs;

namespace Users.Application.Abstractions;

public interface IUserService
{
    Task<Result<TokenResponseDto>> RegisterAsync(RegisterUserRequest request, CancellationToken ct);

    Task<Result<TokenResponseDto>> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);
    Task<Result<string>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
}