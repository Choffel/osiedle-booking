using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.WebApi.Extensions;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthenticationController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterUserRequest createUserDto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(createUserDto, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDto>> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        var result =  await _authService.ConfirmAccountByOtpCode(request, ct);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> LoginAsync([FromBody] AuthRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.AuthenticateAsync(request, cancellationToken);
        return result.ToActionResult();
    }
    
    [HttpPost("booking-otp")]
    public async Task<ActionResult<Guid>> SendBookingOtp([FromBody] SendBookingOtpRequest request, CancellationToken ct)
    {
        var result = await _authService.SendBookingOtpAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        return (await _authService.ForgotPasswordAsync(request, cancellationToken)).ToActionResult();
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<string>> ResetPasswordAsync([FromBody] ResetPasswordDto request, CancellationToken cancellationToken)
    {
        return (await _authService.ResetPasswordAsync(request, cancellationToken)).ToActionResult();
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<string>> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        return (await _authService.ConfirmEmailAsync(request, cancellationToken)).ToActionResult();
    }
}