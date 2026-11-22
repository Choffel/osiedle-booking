using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Employee;
using Business.Domain.Aggregates;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Services.EmployeeServices;

public class EmployeeService : IEmployeeManagementService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _employeeRepository = employeeRepository;
        _businessRepository = businessRepository;
    }
    
   
    public async Task<Result<EmployeeId>> CreateEmployeeAsync(UserId currentUserId, EmployeeCreateDto dto, CancellationToken ct = default)
    {
        var businessId = BusinessId.FromNullable(dto.BusinessId);

        if (businessId is not null)
        {
            var business = await _businessRepository.GetBusinessByIdAsync(businessId.Value, ct);
            if (business is null)
                return Result.Failure<EmployeeId>(EmployeeErrors.BusinessNotFound);
            if (business.OwnerId != currentUserId)
                return Result.Failure<EmployeeId>(EmployeeErrors.Forbidden);
        }

        var employee = Employee.Create(
            businessId,
            dto.FirstName,
            dto.LastName,
            dto.Position,
            dto.Review,
            dto.Email,
            businessId is null ? currentUserId : null);

        var existing = await _employeeRepository.ExistsByEmailAsync(employee.Email, ct);

        if (existing)
        {
            return Result.Failure<EmployeeId>(EmployeeErrors.AlreadyExists);
        }

        _employeeRepository.AddEmployee(employee);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(employee.Id);
    }
    
    public async Task<Result> UpdateProfileAsync(UserId currentUserId, EmployeeId employeeId, UpdateEmployeeProfileDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        if (employee == null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var canManage = await CanManageAsync(employee, currentUserId, ct);
        if (!canManage)
            return Result.Failure(EmployeeErrors.Forbidden);

        var normalizedEmail = Employee.NormalizeEmail(dto.Email);
        
        var isEmailChanged = !string.Equals(employee.Email, normalizedEmail, StringComparison.Ordinal);
    
        if (isEmailChanged && await _employeeRepository.ExistsByEmailAsync(normalizedEmail, ct))
        {
            return Result.Failure(EmployeeErrors.AlreadyExists);
        }
        
        employee.UpdateInfo(dto.FirstName, dto.LastName, dto.Position, dto.Review, normalizedEmail);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> DeactivateEmployeeAsync(UserId currentUserId, EmployeeId employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        
        if (employee == null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var canManage = await CanManageAsync(employee, currentUserId, ct);
        if (!canManage)
            return Result.Failure(EmployeeErrors.Forbidden);
        
        employee.ChangeStatus(EmploymentStatus.Terminated);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<bool> CanManageAsync(Employee employee, UserId currentUserId, CancellationToken ct)
    {
        if (employee.BusinessId is not null)
        {
            var businessId = employee.BusinessId.Value;
            var business = await _businessRepository.GetBusinessByIdAsync(businessId, ct);
            return business?.OwnerId == currentUserId;
        }

        return employee.UserId == currentUserId;
    }
}
