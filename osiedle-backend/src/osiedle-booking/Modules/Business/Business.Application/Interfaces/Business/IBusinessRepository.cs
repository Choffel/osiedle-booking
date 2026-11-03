using Business.Application.DTOs.BusinessDTOs;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Interfaces.Business;

public interface IBusinessRepository
{
    Task <BusinessShortInfoDto?> GetShortInfoAsync(BusinessId businessId, CancellationToken ct);
    
    Task<bool> ExistsByIdAsync(BusinessId businessId, CancellationToken ct);
    
    Task<bool>ExistsByOwnerIdAsync(UserId userId, CancellationToken ct);
    
    Task<Domain.Aggregates.Business?> GetBusinessByIdAsync(BusinessId businessId, CancellationToken ct);
    
    Task<bool> EmployeeBelongsToBusinessAsync(EmployeeId employeeId, BusinessId businessId, CancellationToken ct);
    
    //TODO: relocate to bookable repository
    Task<bool> BookableServiceBelongsToBusinessAsync(BookableServiceId serviceId, BusinessId businessId, CancellationToken ct);
    
    void Add(Domain.Aggregates.Business business, CancellationToken ct);
}
