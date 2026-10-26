using Microsoft.AspNetCore.Mvc;
using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.User.Value_objects;

namespace osiedle_booking.WebApi.Controllers;

public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthenticationController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<Result<UserId>> Register([FromBody] RegisterUserRequest createUserDto, CancellationToken cancellationToken)    
    {
        var result = await _authService.RegisterAsync(createUserDto, cancellationToken);
        return result;
    }

    [HttpPost("login")]
    public async Task<Result<TokenResponseDto>> LoginAsync([FromBody] AuthRequest request, CancellationToken cancellationToken) 
    {
        var result = await _authService.AuthenticateAsync(request, cancellationToken);
        return result;
    }

    [HttpPost("ForgotPassword")]
    public async Task<Result<string>> ForgotPasswordAsync([FromBody ]ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return result;
    }

    [HttpPost("ResetPassword")]
    public async Task<Result<string>> ResetPasswordAsync([FromBody] ResetPasswordDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return result;
    }

    [HttpPost("ConfirmEmail")]
    public async Task<Result<string>> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ConfirmEmailAsync(request, cancellationToken);
        return result;
    }
}