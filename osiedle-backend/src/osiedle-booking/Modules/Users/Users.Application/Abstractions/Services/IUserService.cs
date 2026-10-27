using osiedle_booking.BuildingBlocks;
using Users.Application.DTOs;

namespace Users.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result<string>> UpdateUserAsync(UpdateUserDto userDto, CancellationToken cancellationToken = default);
    Task<Result<CurrentUserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}