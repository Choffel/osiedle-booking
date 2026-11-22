using Business.Application.DTOs.EmployeeDTOs;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Interfaces.Employee;

public interface IEmployeeManagementService
{
    Task<Result<EmployeeId>> CreateEmployeeAsync(UserId currentUserId, EmployeeCreateDto dto, CancellationToken ct = default);
    
    Task<Result<IReadOnlyCollection<EmployeeResponseDto>>> GetAllEmployeesByBusinessIdAsync(BusinessId businessId, CancellationToken ct);
    
    Task<Result> UpdateProfileAsync(UserId currentUserId, EmployeeId employeeId, UpdateEmployeeProfileDto dto, CancellationToken ct);
    
    Task<Result> DeactivateEmployeeAsync(UserId currentUserId, EmployeeId employeeId, CancellationToken ct);
}
