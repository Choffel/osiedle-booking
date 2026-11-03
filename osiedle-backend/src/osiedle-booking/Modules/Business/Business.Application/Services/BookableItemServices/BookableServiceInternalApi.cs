using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.Interfaces.BookableService;
using Business.Domain.Errors;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.BookableItemServices;

public class BookableServiceInternalApi : IBookableServiceInternalApi
{
    private readonly IBookableServiceRepository _bookableServiceRepository;

    public BookableServiceInternalApi(IBookableServiceRepository bookableServiceRepository)
    {
        _bookableServiceRepository = bookableServiceRepository;
    }

    public Task<bool> ExistsAsync(BookableServiceId id, CancellationToken ct = default)
    {
        return _bookableServiceRepository.ExistsAsync(id, ct);
    }

    public Task<bool> BelongsToEmployeeAsync(BookableServiceId id, EmployeeId employeeId, CancellationToken ct = default)
    {
        return _bookableServiceRepository.BelongsToEmployeeAsync(id, employeeId, ct);
    }

    public async Task<Result<ServiceShortInfoDto>> GetServiceInfoAsync(BookableServiceId id, CancellationToken ct = default)
    {
        var serviceInfo = await _bookableServiceRepository.GetBasicInfoByIdAsync(id, ct);

        if (serviceInfo == null)
        {
            return Result.Failure<ServiceShortInfoDto>(BookableServiceErrors.NotFound);
        }

        return Result.Success(serviceInfo);
    }

    public async Task<Result<IReadOnlyCollection<ServiceShortInfoDto>>> GetServicesByEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        var services = await _bookableServiceRepository.GetShortInfosByEmployeeIdAsync(employeeId, ct);

        return Result.Success<IReadOnlyCollection<ServiceShortInfoDto>>(services);
    }
}
