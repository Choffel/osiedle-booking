using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.DTOs.WorkingHoursDTOs;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeInternalApi
{
    Task<bool> IsAvailableForBookingAsync(EmployeeId employeeId, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<WorkingHoursDto>>> GetWorkingHoursAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct = default);
    
    Task<Result<EmployeeShortInfoDto>> GetShortInfoAsync(EmployeeId employeeId, CancellationToken ct = default);
}
