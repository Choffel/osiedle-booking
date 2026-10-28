using Business.Application.DTOs.BusinessDTOs;
using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Interfaces.Business;

public interface IBusinessInternalApi
{
    Task<Result<BusinessShortInfoDto>> GetBusinessInfoAsync(
        BusinessId businessId, 
        CancellationToken ct = default);
    
    Task<bool> EmployeeBelongsToBusinessAsync(
        EmployeeId employeeId, 
        BusinessId businessId, 
        CancellationToken ct = default);

    Task<bool> ServiceBelongsToBusinessAsync(
        BookableServiceId serviceId, 
        BusinessId businessId, 
        CancellationToken ct = default);

    Task<bool> ServiceBelongsToEmployeeAsync(
        BookableServiceId serviceId, 
        EmployeeId employeeId, 
        CancellationToken ct = default);
    
    Task<Result<IReadOnlyCollection<WorkingHoursDto>>> GetWorkingHoursForRangeAsync(
        EmployeeId employeeId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken ct = default);
}