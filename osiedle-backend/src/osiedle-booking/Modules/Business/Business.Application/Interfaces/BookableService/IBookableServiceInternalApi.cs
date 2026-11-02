using Business.Application.DTOs.BookableServiceDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.BookableService;

public interface IBookableServiceInternalApi
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    
    Task<Result<ServiceShortInfoDto>> GetServiceInfoAsync(Guid id, CancellationToken ct = default);
    
    Task<Result<IReadOnlyCollection<ServiceShortInfoDto>>> GetServicesByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
}
