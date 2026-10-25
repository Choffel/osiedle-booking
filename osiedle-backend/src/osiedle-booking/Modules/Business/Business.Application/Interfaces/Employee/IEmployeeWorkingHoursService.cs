using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeWorkingHoursService
{
    Task<Result> SetWorkingHoursAsync(EmployeeId employeeId, SetWorkingHoursDto dto, CancellationToken ct = default);
    
    Task<Result> ClearWorkingHoursAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct = default);
}
