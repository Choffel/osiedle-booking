using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeManagementService
{
    Task<Result<EmployeeId>> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct = default);
    
    Task<Result> UpdateProfileAsync(Guid employeeId, UpdateEmployeeProfileDto dto, CancellationToken ct = default);
    
    Task<Result> DeactivateEmployeeAsync(Guid employeeId, CancellationToken ct = default);
}
