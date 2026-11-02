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
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(
        IEmployeeReadOnlyRepository employeeReadOnlyRepository,
        IEmployeeRepository employeeRepository,
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _employeeRepository = employeeRepository;
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _businessRepository = businessRepository;
    }
    
   public async Task<Result<Guid>> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct = default)
    {
        var businessId = dto.BusinessId is null
            ? (BusinessId?)null
            : BusinessId.From(dto.BusinessId.Value);

        if (businessId is not null)
        {
            bool businessExists = await _businessRepository.ExistsByIdAsync(businessId.Value, ct);
            if (!businessExists)
            {
                return Result.Failure<Guid>(EmployeeErrors.BusinessNotFound);
            }
        }
        
        var employee = Employee.Create(
            businessId,
            EmployeeName.Create(dto.FirstName, dto.LastName),
            dto.Position,
            dto.Review,
            dto.Email
        );

        _employeeRepository.Add(employee);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(employee.Id.Value);
    }
    
    public async Task<Result> UpdateProfileAsync(Guid employeeId, UpdateEmployeeProfileDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(EmployeeId.From(employeeId), ct);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }
        
        if (!string.Equals(employee.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool isEmailTaken = await _employeeReadOnlyRepository.ExistsByEmailAsync(dto.Email, ct);
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

    public async Task<Result> DeactivateEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(EmployeeId.From(employeeId), ct);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }
        employee.ArchiveEmployee();
        
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
