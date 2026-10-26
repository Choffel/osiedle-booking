using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using Users.Domain.Entity.User.Value_objects;
using Microsoft.Extensions.Logging;
using osiedle_booking.BuildingBlocks.Interfaces;
using Users.Application.Abstractions.Repositories;
using Users.Application.DTOs;

namespace Users.Application.Services;

public class UserService : IUserService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IRoleService _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IUserContext _userContext;

    public UserService(IIdentityService identityService, IUserRepository userRepository, ILogger<UserService> logger,
        IRoleService roleManager, ITokenService tokenService, IUserContext userContext)
    {
        _identityService = identityService; 
        _userRepository = userRepository;
        _logger = logger;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _userContext = userContext;
    }
    
    public async Task<Result<string>> UpdateUserAsync(UpdateUserDto userDto, CancellationToken cancellationToken = default)
    {
        var userEmail = _userContext.Email;
        var userContextId = _userContext.UserId;
        
        var user = await _identityService.FindByEmailAsync(userEmail, cancellationToken);
        if (user == null)
        {
            return Result<string>.Failure("User not found");
        }
        
        var userId = new UserId(userContextId);

        var domainUser = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (domainUser is null)
        {
            return Result<string>.Failure("User not found");
        }

        domainUser.Update(userDto.UserName);
        await _identityService.UpdateUserNameAsync(userContextId, userDto.UserName, cancellationToken);

        await _userRepository.Update(domainUser);

        return Result<string>.Success("User updated successfully");
    }

    public async Task<Result<CurrentUserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        
        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken);
        if (user == null)
        {
            return Result<CurrentUserDto>.Failure("User not found");
        }

        return Result<CurrentUserDto>.Success(new CurrentUserDto(user.DisplayName));
    }
}