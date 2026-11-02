using Business.Application.DTOs.EmployeeDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeManagementService
{
    Task<Result<Guid>> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct = default);
    
    Task<Result> UpdateProfileAsync(Guid employeeId, UpdateEmployeeProfileDto dto, CancellationToken ct);
    
    Task<Result> DeactivateEmployeeAsync(Guid employeeId, CancellationToken ct);
}
