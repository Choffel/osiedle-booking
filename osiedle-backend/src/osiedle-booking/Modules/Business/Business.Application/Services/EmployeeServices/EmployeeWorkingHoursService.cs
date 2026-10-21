using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Employee;
using Business.Domain.Entities;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.EmployeeServices;

public class EmployeeWorkingHoursService : IEmployeeWorkingHoursService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeWorkingHoursRepository _employeeWorkingHoursRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeWorkingHoursService(
        IEmployeeRepository employeeRepository,
        IEmployeeWorkingHoursRepository employeeWorkingHoursRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _employeeWorkingHoursRepository = employeeWorkingHoursRepository;
        _unitOfWork = unitOfWork;
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
        
        var workingHours = await _employeeWorkingHoursRepository.GetWorkingHoursByDateAsync(employeeId, dto.Date, ct);
        
        if (workingHours == null)
        {
            _employeeWorkingHoursRepository.AddWorkingHours(WorkingHours.Create(employeeId, dto.Date, dto.StartTime, dto.EndTime));
            
        }
        else
        {
            workingHours.UpdateTime(dto.StartTime, dto.EndTime);
            _employeeWorkingHoursRepository.UpdateWorkingHours(workingHours);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ClearWorkingHoursAsync(Guid employeeId, DateOnly date, CancellationToken ct = default)
    {
        var id = EmployeeId.From(employeeId);
        
        var workingHours = await _employeeWorkingHoursRepository.GetWorkingHoursByDateAsync(id, date, ct);
        
        if (workingHours != null)
        {
            _employeeWorkingHoursRepository.RemoveWorkingHours(workingHours);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return Result.Success();
    }
}
