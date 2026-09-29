using osiedle_booking.BuildingBlocks;
using Users.Appliation.Abstractions;
using Users.Appliation.DTOs;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.Value_objects;
using Microsoft.Extensions.Logging;

namespace Users.Appliation.Services;

public class UserService : IUserService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IRoleService _roleManager;
    private readonly ITokenService _tokenService;

    public UserService(IIdentityService identityService, IUserRepository userRepository, ILogger<UserService> logger, IRoleService roleManager)
    {
        _identityService = identityService; 
        _userRepository = userRepository;
        _logger = logger;
        _roleManager = roleManager;
    }
    
    public async Task<Result<TokenResponseDto>> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
    {
        //await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        
        var identityResult = await _identityService.CreateIdentityAsync(
            request.Email, request.Password, ct);

        if (identityResult.IsSuccess == false)
            return Result<TokenResponseDto>.Failure(identityResult.Error);

        
        var userId = new UserId(identityResult.Value);
        var user = User.Create(userId, request.FirstName);

        await _userRepository.AddAsync(user, ct);
        //await _unitOfWork.SaveChangesAsync(ct);
        //await transaction.CommitAsync(ct); // если ваш IUnitOfWork это поддерживает
        var token = await _identityService.GenerateEmailConfirmationTokenAsync(identityResult.Value, ct);
        
        _logger.LogInformation("Email confirmation token for user {UserId}: {Token}", user.Id, token);

        var response = new TokenResponseDto
        (
            AccessToken: string.Empty,
            RefreshToken: string.Empty
        );

        return Result<TokenResponseDto>.Success(response);
    }
    
    public async Task<Result<TokenResponseDto>> AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<TokenResponseDto>.Failure("Invalid Email or Password");
        }

        if (!user.EmailConfirmed)
        {
            return Result<TokenResponseDto>.Failure("Email not confirmed");
        }

        var isPasswordCorrect = await _identityService.CheckPasswordAsync(user.Id, request.Password, cancellationToken);
        if (!isPasswordCorrect)
        {
            return Result<TokenResponseDto>.Failure("Invalid Email or Password");
        }

        var userDto = new UserDto(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty);

        var tokens = await _tokenService.GetTokens(userDto, cancellationToken);

        return Result<TokenResponseDto>.Success(new TokenResponseDto(tokens.AccessToken, tokens.RefreshToken));
    }
    
    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<string>.Success("If this email address exists in the system, instructions have been sent to the email.");
        }

        var token = await _identityService.GeneratePasswordResetTokenAsync(user.Id, cancellationToken);
        
        _logger.LogInformation("Password reset token for user {UserId}: {Token}", user.Id, token);

        return Result<string>.Success("If this email address exists in the system, instructions have been sent to the email.");
    }
    
    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<string>.Failure($"{request.Email} - this email address is not registered");
        }

        var resetPassResult = await _identityService.ResetPasswordAsync(user.Id, request.Token, request.NewPassword!, cancellationToken);

        if (!resetPassResult.Succeeded)
        {
            return Result<string>.Failure("Failed to reset password");
        }

        return Result<string>.Success("Password reset successfully");
    }
    
    public async Task<Result<string>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<string>.Failure("Invalid email address");
        }

        var result = await _identityService.ConfirmEmailAsync(user.Id, request.Token, cancellationToken);
        if (!result)
        {
            return Result<string>.Failure("Invalid or expired confirmation token");
        }

        var roleExists = await _roleManager.RoleExistsAsync("User", cancellationToken);
        if (!roleExists)
        {
            return Result<string>.Failure("Role 'User' does not exist");
        }

        await _identityService.AddToRoleAsync(user.Id, "User", cancellationToken);

        return Result<string>.Success("Email confirmed");
    }
    
    public async Task<Result<string>> UpdateUserAsync(UpdateUserDto userDto, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByEmailAsync(userDto.Email, cancellationToken);
        if (user == null)
        {
            return Result<string>.Failure("User not found");
        }
        
        
        var userId = new UserId(userDto.Id);

        var domainUser = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (domainUser is null)
        {
            return Result<string>.Failure("User not found");
        }

        domainUser.Update(userDto.UserName);

        _userRepository.Update(domainUser);
        

        return Result<string>.Success("User updated successfully");
    }
}