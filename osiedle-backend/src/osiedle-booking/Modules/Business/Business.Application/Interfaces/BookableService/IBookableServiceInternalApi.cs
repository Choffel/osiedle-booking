using Business.Application.DTOs.BookableServiceDTOs;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.BookableService;

public interface IBookableServiceInternalApi
{
    Task<bool> ExistsAsync(BookableServiceId id, CancellationToken ct = default);
    
    Task<bool> BelongsToEmployeeAsync(BookableServiceId id, EmployeeId employeeId, CancellationToken ct = default);
    
    Task<Result<ServiceShortInfoDto>> GetServiceInfoAsync(BookableServiceId id, CancellationToken ct = default);
    
    Task<Result<IReadOnlyCollection<ServiceShortInfoDto>>> GetServicesByEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default);
}
