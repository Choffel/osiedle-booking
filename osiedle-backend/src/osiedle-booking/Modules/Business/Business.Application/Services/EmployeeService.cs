using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Employee;
using Business.Domain.Aggregates;
using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services;

public class EmployeeService : IEmployeeManagementService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _employeeRepository = employeeRepository;
    }
    
    public async Task<Result<EmployeeId>> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct = default)
    {
        var normalizedEmail = dto.Email.ToLower().Trim();
        
        var existing = await _employeeRepository.ExistsByEmailAsync(normalizedEmail, ct);

        if (existing)
        {
            return Result.Failure<EmployeeId>(EmployeeErrors.AlreadyExists);
        }
        
        var businessId = BusinessId.FromNullable(dto.BusinessId);
        
        var employee =  Employee.Create(businessId,dto.FirstName,dto.LastName, dto.Position, dto.Review, normalizedEmail );

         _employeeRepository.AddEmployee(employee);

         await _unitOfWork.SaveChangesAsync(ct);
         
         return Result.Success(employee.Id);
    }
    
    public async Task<Result> UpdateProfileAsync(UpdateEmployeeProfileDto dto, CancellationToken ct = default)
    {
        var employeeId = EmployeeId.From(dto.EmployeeId);
        
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        
        if (employee == null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }
        
        employee.UpdateInfo(dto.FirstName, dto.LastName, dto.Position, dto.Review, dto.Email);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeactivateEmployeeAsync(EmployeeId id, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(id, ct);
        
        if (employee == null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }
        
        employee.ChangeStatus(EmploymentStatus.Terminated);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> SetWorkingHoursAsync(SetWorkingHoursDto dto, CancellationToken ct = default)
    {
        if (dto.EndTime <= dto.StartTime)
        {
            return Result.Failure(EmployeeErrors.WorkingHoursInvalid);
        }
        
        var employeeId = EmployeeId.From(dto.EmployeeId);
        
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        
        if (employee == null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }
        
        var workingHours = await _employeeRepository.GetWorkingHoursAsync(employeeId, dto.Date, ct);
        
        if (workingHours == null)
        {
            _employeeRepository.AddWorkingHours(WorkingHours.Create(employeeId, dto.Date, dto.StartTime, dto.EndTime));
            
        }
        else
        {
            workingHours.UpdateTime(dto.StartTime, dto.EndTime);
            _employeeRepository.UpdateWorkingHours(workingHours);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ClearWorkingHoursAsync(Guid employeeId, DateOnly date, CancellationToken ct = default)
    {
        var id = EmployeeId.From(employeeId);
        
        var workingHours = await _employeeRepository.GetWorkingHoursAsync(id, date, ct);
        
        if (workingHours != null)
        {
            _employeeRepository.RemoveWorkingHours(workingHours);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return Result.Success();
    }
}