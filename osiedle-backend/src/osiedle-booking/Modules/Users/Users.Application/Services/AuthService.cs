using Microsoft.Extensions.Logging;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Services;
using Users.Application.DTOs;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.Value_objects;

namespace Users.Application.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly IRoleService _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IUserContext _userContext;

    public AuthService(IIdentityService identityService, IUserRepository userRepository, ILogger<AuthService> logger,
        IRoleService roleManager, ITokenService tokenService, IUserContext userContext)
    {
        _identityService = identityService; 
        _userRepository = userRepository;
        _logger = logger;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _userContext = userContext;
    }
    
     public async Task<Result<UserId>> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
    {
        var identityResult = await _identityService.CreateIdentityAsync(
            request.Email,  request.FirstName, request.Password, ct);

        if (!identityResult.IsSuccess)
            return Result<UserId>.Failure(identityResult.Error);
        
        var userId = new UserId(identityResult.Value);
        var user = User.Create(userId, request.FirstName);

        await _userRepository.AddAsync(user, ct);
        
        var token = await _identityService.GenerateEmailConfirmationTokenAsync(identityResult.Value, ct);
        
        _logger.LogInformation("Email confirmation token for user {UserId}: {Token}", user.Id, token); // TODO: Fix this when the notification service becomes available 

        return Result<UserId>.Success(userId);
    }
    
    public async Task<Result<TokenResponseDto>> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default)
    {
        
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<TokenResponseDto>.Failure("Invalid Email or Password");
        }

        if (!user.IsSuccess)
        {
            return Result<TokenResponseDto>.Failure(user.Error);
        }

        var isPasswordCorrect = await _identityService.CheckPasswordAsync(user.Value.Id, request.Password, cancellationToken);
        if (!isPasswordCorrect.IsSuccess)
        {
            _logger.LogWarning("Failed login for {Email} from {Ip}", request.Email, _userContext.IpAddress);
            return Result<TokenResponseDto>.Failure(isPasswordCorrect.Error);
        }

        var userDto = new UserDto(user.Value.Id, user.Value.UserName ?? string.Empty, user.Value.Email ?? string.Empty);

        var tokens = await _tokenService.GetTokens(userDto, cancellationToken);

        return Result<TokenResponseDto>.Success(new TokenResponseDto(tokens.AccessToken, tokens.RefreshToken));
    }
    
    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if(!user.IsSuccess)
        {
            return Result<string>.Success("If this email address exists in the system, instructions have been sent to the email.");
        }

        var token = await _identityService.GeneratePasswordResetTokenAsync(user.Value.Id, cancellationToken);
        if(!token.IsSuccess)
        {
            return Result<string>.Failure(token.Error);
        }

        _logger.LogInformation("Password reset token for user {UserId}: {Token}", user.Value.Id, token); // TODO: удалить логирование токена и перенести его отправку на почту когда появиться сервис нотификации

        return Result<string>.Success("If this email address exists in the system, instructions have been sent to the email.");
    }
    
    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (!user.IsSuccess)
        {
            return Result<string>.Failure($"{request.Email} - this email address is not registered");
        }

        var resetPassResult = await _identityService.ResetPasswordAsync(user.Value.Id, request.Token, request.NewPassword!, cancellationToken);

        if (!resetPassResult.IsSuccess)
        {
            return Result<string>.Failure("Failed to reset password");
        }

        return Result<string>.Success("Password reset successfully");
    }
    
    public async Task<Result<string>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (!user.IsSuccess)
        {
            return Result<string>.Failure("Invalid email address");
        }

        var result = await _identityService.ConfirmEmailAsync(user.Value.Id, request.Token, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<string>.Failure("Invalid or expired confirmation token");
        }

        var roleExists = await _roleManager.RoleExistsAsync("User", cancellationToken);
        if (!roleExists)
        {
            return Result<string>.Failure("Role 'User' does not exist");
        }

        await _identityService.AddToRoleAsync(user.Value.Id, "User", cancellationToken);

        return Result<string>.Success("Email confirmed");
    }
}