using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Employee;
using Business.Domain.Aggregates;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

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
        if (dto.BusinessId is not null)
        {
            bool businessExists = await _businessRepository.ExistsByIdAsync(dto.BusinessId.Value, ct);
            if (!businessExists)
            {
                return Result.Failure<EmployeeId>(EmployeeErrors.BusinessNotFound);
            }
        }
        
        var employee = Employee.Create(
            dto.BusinessId,
            dto.Name,
            dto.Position,
            dto.Review,
            dto.Email
        );

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
        
        if (!string.Equals(employee.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool isEmailTaken = await _employeeRepository.ExistsByEmailAsync(dto.Email, ct);
            if (isEmailTaken)
            {
                return Result.Failure(EmployeeErrors.AlreadyExists);
            }

            employee.ChangeEmail(dto.Email);
        }
        
        
        var newName = EmployeeName.Create(dto.FirstName, dto.LastName);
        if (employee.EmployeeName != newName)
        {
            employee.UpdateEmployeeName(newName);
        }
        
        if (!string.Equals(employee.Position, dto.Position, StringComparison.Ordinal))
        {
            employee.ChangePosition(dto.Position);
        }
        
        
        employee.ChangeReview(dto.Review);
        
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
        employee.ArchiveEmployee();
        
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
