using Business.Application.DTOs.BookableServiceDTOs;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;


namespace Business.Application.Interfaces.BookableService;

public interface IBookableServiceRepository 
{
    Task <Domain.Entities.BookableService> GetByIdAsync(BookableServiceId id, CancellationToken ct);
    Task <ServiceShortInfoDto> GetBasicInfoByIdAsync(BookableServiceId id, CancellationToken ct);
    Task <IReadOnlyList<ServiceShortInfoDto>> GetShortInfosByEmployeeIdAsync(EmployeeId employeeId, CancellationToken ct);
    
    Task<bool> ExistsAsync(BookableServiceId id, CancellationToken ct);
    Task<bool> BelongsToEmployeeAsync(BookableServiceId id, EmployeeId employeeId, CancellationToken ct);
    Task<bool> ExistsByNameForEmployeeAsync(string serviceName, EmployeeId employeeId, CancellationToken ct);
    
    void Add(Domain.Entities.BookableService service, CancellationToken ct);
}
