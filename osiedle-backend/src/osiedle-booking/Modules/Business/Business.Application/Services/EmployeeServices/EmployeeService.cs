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
    
    public async Task<Result<EmployeeId>> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct = default)
    {
        var businessId = BusinessId.FromNullable(dto.BusinessId);

        if (businessId is not null)
        {
            var businessExists = await _businessRepository.ExistsByIdAsync(businessId.Value, ct);

            if (!businessExists)
            {
                return Result.Failure<EmployeeId>(EmployeeErrors.BusinessNotFound);
            }
        }

        var employee = Employee.Create(
            businessId,
            dto.FirstName,
            dto.LastName,
            dto.Position,
            dto.Review,
            dto.Email);

        var existing = await _employeeRepository.ExistsByEmailAsync(employee.Email, ct);

        if (existing)
        {
            return Result.Failure<EmployeeId>(EmployeeErrors.AlreadyExists);
        }

        _employeeRepository.AddEmployee(employee);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(employee.Id);
    }
    
    public async Task<Result> UpdateProfileAsync(EmployeeId employeeId, UpdateEmployeeProfileDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        if (employee == null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

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

    public async Task<Result> DeactivateEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        
        if (employee == null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }
        
        employee.ChangeStatus(EmploymentStatus.Terminated);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
