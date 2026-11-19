using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeWorkingHoursService
{
    Task<Result> SetWorkingHoursAsync(UserId currentUserId, EmployeeId employeeId, SetWorkingHoursDto dto, CancellationToken ct = default);
    
    Task<Result> ClearWorkingHoursAsync(UserId currentUserId, EmployeeId employeeId, DateOnly date, CancellationToken ct = default);
}
