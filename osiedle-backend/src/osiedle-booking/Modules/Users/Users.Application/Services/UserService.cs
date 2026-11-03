using osiedle_booking.BuildingBlocks;
using Users.Application.Abstractions.Services;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Application.Abstractions;
using Users.Application.Abstractions.Repositories.Users;
using Users.Application.Abstractions.Services.Identity;
using Users.Application.DTOs;
using Users.Domain.Entity.User.ValueObjects;
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
    
    public async Task<Result> UpdateUserNameAsync(UserId userId, string email, UpdateUserDto userDto,
        CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByEmailAsync(email, ct);
        if (user.IsFailure)
            return Result.Failure(UserErrors.NotFound);

        var domainUser = await _userRepository.GetByIdAsync(userId, ct);
        if (domainUser is null)
            return Result.Failure(UserErrors.NotFound);
        domainUser.UpdateName(Name.Create(userDto.FirstName, userDto.LastName));

        var updateResult = await _identityAccountService.UpdateUserNameAsync(userId, userDto.FirstName, ct);
        if (updateResult.IsFailure)
            return Result.Failure(UserErrors.UpdateFailed);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
    
    public async Task<Result> UpdateUserEmailAsyc(UserId userId, string email, CancellationToken ct = default)
    {
        var user = await _identityAccountService.FindByUserIdAsync(userId, ct);
        if (user.IsFailure)
            return Result.Failure(UserErrors.NotFound);

        var domainUser = await _userRepository.GetByIdAsync(userId, ct);
        if (domainUser is null)
            return Result.Failure(UserErrors.NotFound);
        domainUser.UpdateEmail(email);

        var updateResult = await _identityAccountService.UpdateUserEmailAsync(userId, email, ct);
        if (updateResult.IsFailure)
            return Result.Failure(UserErrors.UpdateFailed);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<CurrentUserDto>> GetUserByIdAsync(UserId userId, CancellationToken ct = default)
    {
        var user = await _userReadRepository.GetProfileAsync(userId, ct);
        if (user is null)
            return Result<CurrentUserDto>.Failure(UserErrors.NotFound);

        return Result<CurrentUserDto>.Success(new CurrentUserDto(user.FirstName, user.LastName));
    }
}