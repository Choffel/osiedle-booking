using Microsoft.Extensions.Logging;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.Roles;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;
using Users.Domain.Errors;

namespace Users.Application.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityCredentialsService _identityCredentialsService;
    private readonly IIdentityAccountService _identityAccountService;
    private readonly IIdentityEmailService _identityEmailService;
    private readonly ILogger<AuthService> _logger;
    private readonly IRoleService _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService( IIdentityCredentialsService identityCredentialsService,
        IIdentityAccountService identityAccountService, IIdentityEmailService identityEmailService,
        ILogger<AuthService> logger, IRoleService roleManager, ITokenService tokenService, IUserContext userContext,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _identityCredentialsService = identityCredentialsService; 
        _identityAccountService = identityAccountService;
        _identityEmailService = identityEmailService;
        _logger = logger;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _userContext = userContext;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    
     public async Task<Result> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var identityResult = await _identityAccountService.CreateIdentityAsync(
            request.Email,  request.FirstName, request.Password, ct);

        if (identityResult.IsFailure)
            return Result.Failure(identityResult.Error); // TODO: обратить внимание на возвращаемый текст
        
        var user = User.Create(UserId.From(identityResult.Value.Value), Name.Create(request.FirstName, request.LastName),  request.Email);

        _userRepository.Add(user);
        
        var token = await _identityEmailService.GenerateEmailConfirmationTokenAsync(user.Id, ct);
        
        _logger.LogInformation("Email confirmation token for user {UserId}: {Token}", user.Id, token); // TODO: Fix this when the notification service becomes available 

        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(user.Id);
    }
    
    public async Task<Result> AuthenticateAsync(AuthRequest request, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);

        if (user.IsFailure)
            return Result.Failure(user.Error);
        

        var isPasswordCorrect = await _identityCredentialsService.CheckPasswordAsync(UserId.From(user.Value.Id),
            request.Password, ct);
        
        if (isPasswordCorrect.IsFailure)
            return Result.Failure(isPasswordCorrect.Error);
        
        
        var tokens = await _tokenService.GenerateTokensAsync(UserId.From(user.Value.Id),
            user.Value.Email, user.Value.Roles, ct);

        return Result.Success(tokens);
    }
    
    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);
        if (user.IsFailure)
        {
            _logger.LogError(user.Error.Description);
            return Result.Success();
        }
        
        var token = await _identityCredentialsService.GeneratePasswordResetTokenAsync(UserId.From(user.Value.Id),ct);
        
        _logger.LogInformation("Password reset token for user {UserId}: {Token}", user.Value.Id, token); // TODO: удалить логирование токена и перенести его отправку на почту когда появиться сервис нотификации

        return Result.Success();
    }
    
    public async Task<Result> ResetPasswordAsync(ResetPasswordDto request, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);
        if (user.IsFailure)
            return Result.Failure(UserErrors.EmailNotRegistered); // TODO: пройтись везде по тексту ответа
        
        var resetPassResult = await _identityCredentialsService.ResetPasswordAsync(UserId.From(user.Value.Id),
            request.Token, request.NewPassword!, ct);

        if (resetPassResult.IsFailure)
            return Result.Failure(UserErrors.PasswordResetFailed);
        
        return Result.Success();
    }
    
    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);
        if (user.IsFailure)
            return Result.Failure(UserErrors.EmailNotRegistered);
        
        var result = await _identityEmailService.ConfirmEmailAsync(UserId.From(user.Value.Id), request.Token, ct);
        if (result.IsFailure)
            return Result.Failure(UserErrors.EmailConfirmationFailed);

        var roleResponse = await _roleManager.AddToRoleAsync(UserId.From(user.Value.Id),UserRoles.User, ct);
        if(roleResponse.IsFailure)
            return Result.Failure(roleResponse.Error);

        return Result.Success();
    }
}