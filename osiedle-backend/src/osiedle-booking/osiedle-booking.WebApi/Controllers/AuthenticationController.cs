using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.WebApi.Extensions;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;

namespace osiedle_booking.WebApi.Controllers;

public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthenticationController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest createUserDto, CancellationToken cancellationToken)    
    {
        var result = await _authService.RegisterAsync(createUserDto, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] AuthRequest request, CancellationToken cancellationToken) 
    {
        var result = await _authService.AuthenticateAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody ]ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ConfirmEmailAsync(request, cancellationToken);
        return result.ToActionResult();
    }
}