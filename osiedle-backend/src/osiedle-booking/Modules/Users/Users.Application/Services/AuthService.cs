using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notifications.Application.Interfaces;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.Roles;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.Abstractions.Services.Tokens;
using Users.Application.Errors;
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
    private readonly INotificationInternalApi _notificationInternalApi;
    private readonly IConfiguration _configuration;

    public AuthService( IIdentityCredentialsService identityCredentialsService,
        IIdentityAccountService identityAccountService, IIdentityEmailService identityEmailService,
        ILogger<AuthService> logger, IRoleService roleManager, ITokenService tokenService, IUserContext userContext,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator,
        INotificationInternalApi notificationInternalApi, IConfiguration configuration)
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
        _notificationInternalApi = notificationInternalApi;
        _configuration = configuration;
    }
    
     public async Task<Result<Guid>> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var identityResult = await _identityAccountService.CreateIdentityAsync(
            request.Email,  request.Email,  ct);

        if (identityResult.IsFailure)
            return Result<Guid>.Failure(identityResult.Error); // TODO: обратить внимание на возвращаемый текст
        
        var user = User.Create(UserId.From(identityResult.Value.UserId), Name.Create(request.FirstName, request.LastName),  request.Email);

        _userRepository.Add(user);
        
        var token = await _identityEmailService.GenerateEmailConfirmationTokenAsync(user.Id, ct);

        if (token.IsFailure)
            return Result<Guid>.Failure(token.Error);

        var confirmationLink = CreateEmailConfirmationLink(request.Email, token.Value);
        await _notificationInternalApi.SendVerificationLinkAsync(request.Email, confirmationLink, ct);

        _logger.LogInformation($"User confirm code: {identityResult.Value.code}");

        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result<Guid>.Success(user.Id.Value);
    }
     
    public async Task<Result<Guid>> SendBookingOtpAsync(
        SendBookingOtpRequest request,
        CancellationToken ct = default)
    {
        var otpResult = await _identityAccountService.GetOrCreateAndGenerateOtpAsync(request.Email, ct);
        if (otpResult.IsFailure)
            return Result<Guid>.Failure(otpResult.Error);

        var (userId, code, isNewUser) = otpResult.Value;

        // Создаём доменного User только если это первый визит клиента
        if (isNewUser)
        {
            var user = User.Create(
                userId,
                Name.Create(request.FirstName.Trim(), request.LastName.Trim()),
                request.Email.Trim());

            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // Отправляем 6-значный код на почту вместо длинной ссылки
        await _notificationInternalApi.SendVerificationLinkAsync(request.Email, code, ct);

        _logger.LogInformation("Booking OTP code for {Email}: {Code}", request.Email, code);

        return Result<Guid>.Success(userId.Value);
    }
     
    public async Task<Result<TokenResponseDto>> ConfirmAccountByOtpCode(VerifyOtpRequest request, CancellationToken ct = default)
    {
        var isOtpValid = await _identityAccountService.VerifyEmailConfirmationTokenAsync(request.Email, request.Code, ct);
        if (isOtpValid.IsFailure)
            return Result<TokenResponseDto>.Failure(IdentityErrors.InvalidOtp);
        
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);
        if (user.IsFailure)
            return Result<TokenResponseDto>.Failure(IdentityErrors.NotFound);

        var tokens = await _tokenService.GenerateTokensAsync(UserId.From(user.Value.Id),
            user.Value.Email, user.Value.Roles, ct);
        
        return Result<TokenResponseDto>.Success(tokens.Value);
    }

    private string CreateEmailConfirmationLink(string email, string token)
    {
        var frontendDomain = _configuration["Frontend:Domain"]?.TrimEnd('/')
                             ?? throw new InvalidOperationException("Frontend:Domain is missing in configuration.");

        return $"{frontendDomain}/Authentication/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
    }
    
    public async Task<Result<TokenResponseDto>> AuthenticateAsync(AuthRequest request, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);

        if (user.IsFailure)
            return Result<TokenResponseDto>.Failure(IdentityErrors.InvalidCredentials); // одинаково с неверным паролем — без утечки enumeration
        

        var isPasswordCorrect = await _identityCredentialsService.CheckPasswordAsync(UserId.From(user.Value.Id),
            request.Password, ct);
        
        if (isPasswordCorrect.IsFailure)
            return Result<TokenResponseDto>.Failure(isPasswordCorrect.Error);
        
        
        var tokens = await _tokenService.GenerateTokensAsync(UserId.From(user.Value.Id),
            user.Value.Email, user.Value.Roles, ct);

        return Result<TokenResponseDto>.Success(tokens.Value);
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
        
        _logger.LogInformation("Password reset token for user {UserId}: {Token}", user.Value.Id, token.Value); // TODO: удалить логирование токена и перенести его отправку на почту когда появится сервис нотификации

        return Result.Success();
    }
    
    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);
        if (user.IsFailure)
            return Result<string>.Failure(UserErrors.EmailNotRegistered); // TODO: пройтись везде по тексту ответа
        
        var resetPassResult = await _identityCredentialsService.ResetPasswordAsync(UserId.From(user.Value.Id),
            request.Token, request.NewPassword!, ct);

        if (resetPassResult.IsFailure)
            return Result<string>.Failure(UserErrors.PasswordResetFailed);
        
        return Result<string>.Success("The password was successfully recovered.");
    }
    
    public async Task<Result<string>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(request.Email, ct);
        if (user.IsFailure)
            return Result<string>.Failure(UserErrors.EmailNotRegistered);
        
        var result = await _identityEmailService.ConfirmEmailAsync(UserId.From(user.Value.Id), request.Token, ct);
        if (result.IsFailure)
            return Result<string>.Failure(UserErrors.EmailConfirmationFailed);

        var roleResponse = await _roleManager.AddToRoleAsync(UserId.From(user.Value.Id),UserRoles.User, ct);
        if(roleResponse.IsFailure)
            return Result<string>.Failure(roleResponse.Error);

        return Result<string>.Success("Email confirmed successfully.");
    }
}