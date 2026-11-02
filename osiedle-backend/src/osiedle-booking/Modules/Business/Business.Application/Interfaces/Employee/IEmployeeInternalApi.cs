using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.DTOs.WorkingHoursDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeInternalApi
{
    Task<Result<bool>> IsAvailableForBookingAsync(Guid employeeId, CancellationToken ct = default);
    Task<Result<WorkingHoursDto>> GetWorkingHoursAsync(Guid employeeId, DateOnly date, CancellationToken ct = default);
    
    Task<Result<EmployeeShortInfoDto>> GetShortInfoAsync(Guid employeeId, CancellationToken ct = default);
}
