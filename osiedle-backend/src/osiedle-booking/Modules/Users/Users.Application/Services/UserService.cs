using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;
using Users.Domain.Errors;

namespace Users.Application.Services;

public class UserService : IUserService
{
    private readonly IIdentityAccountService _identityAccountService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserReadRepository _userReadRepository;

    public UserService(IIdentityAccountService identityAccountService,IUserRepository userRepository,
        IUnitOfWork unitOfWork, IUserReadRepository userReadRepository)
    {
        _identityAccountService = identityAccountService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userReadRepository = userReadRepository;
    }
    
    public async Task<Result> UpdateUserAsync(UserId userId, string email, UpdateUserDto userDto,
        CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(email, ct);
        if (user.IsFailure)
            return Result.Failure(UserErrors.NotFound);

        var domainUser = await _userRepository.GetByIdAsync(userId, ct);

        if (domainUser is null)
            return Result.Failure(UserErrors.NotFound);

        var userProfile = await _userReadRepository.GetProfileAsync(userId, ct);
        
        domainUser.Update(userProfile.FirstName, userProfile.LastName);
        //await _identityAccountService.UpdateUserNameAsync(userId, userDto.UserName, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> GetUserByIdAsync(UserId userId, CancellationToken ct = default)
    {
        var user = await _userReadRepository.GetProfileAsync(userId, ct);
        if (user is null) 
            return Result.Failure(UserErrors.NotFound);
        
        return Result.Success(new CurrentUserDto(user.FirstName, user.LastName));
    }
}