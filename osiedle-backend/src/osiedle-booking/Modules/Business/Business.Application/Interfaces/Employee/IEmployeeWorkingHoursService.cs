using Business.Application.DTOs.WorkingHoursDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeWorkingHoursService
{
    Task<Result> SetWorkingHoursAsync(Guid employeeId, SetWorkingHoursDto dto, CancellationToken ct = default);
    
    Task<Result> ClearWorkingHoursAsync(Guid employeeId, DateOnly date, CancellationToken ct = default);
}
