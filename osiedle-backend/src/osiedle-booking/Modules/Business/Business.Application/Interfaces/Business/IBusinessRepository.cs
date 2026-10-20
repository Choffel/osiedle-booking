using Business.Application.DTOs.BusinessDTOs;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Application.Interfaces.Business;

public interface IBusinessRepository
{
    Task <BusinessShortInfoDto?> GetShortInfoAsync(BusinessId businessId, CancellationToken ct);
    
    Task<bool>ExistsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    
    Task<Domain.Aggregates.Business?> GetBusinessByIdAsync(BusinessId businessId, CancellationToken ct);
    
    Task<bool> EmployeeBelongsToBusinessAsync(EmployeeId employeeId, BusinessId businessId, CancellationToken ct);
    
    Task<bool> BookableServiceBelongsToBusinessAsync(BookableServiceId serviceId, BusinessId businessId, CancellationToken ct);
    
    Task<bool> BookableServiceBelongsToEmployeeAsync(BookableServiceId serviceId, EmployeeId employeeId, CancellationToken ct);
    

    void Add(Domain.Aggregates.Business business, CancellationToken ct);
}
