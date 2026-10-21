using Business.Application.DTOs.EmployeeDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeWorkingHoursService
{
    Task<Result> SetWorkingHoursAsync(SetWorkingHoursDto dto, CancellationToken ct = default);
    
    Task<Result> ClearWorkingHoursAsync(Guid employeeId, DateOnly date, CancellationToken ct = default);
}
