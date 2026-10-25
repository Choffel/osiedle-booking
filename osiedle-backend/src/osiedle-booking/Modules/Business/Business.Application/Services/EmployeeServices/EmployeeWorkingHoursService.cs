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

    public async Task<Result> SetWorkingHoursAsync(EmployeeId employeeId, SetWorkingHoursDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var workingHours = await _employeeWorkingHoursRepository.GetWorkingHoursByDateAsync(employeeId, dto.Date, ct);

        if (workingHours is null)
        {
            var newWorkingHours = WorkingHours.Create(employeeId, dto.Date, dto.StartTime, dto.EndTime);
            _employeeWorkingHoursRepository.AddWorkingHours(newWorkingHours);
        }
        else
        {
            workingHours.UpdateTime(dto.StartTime, dto.EndTime);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ClearWorkingHoursAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct = default)
    {
        var workingHours = await _employeeWorkingHoursRepository.GetWorkingHoursByDateAsync(employeeId, date, ct);
        
        if (workingHours != null)
        {
            _employeeWorkingHoursRepository.RemoveWorkingHours(workingHours);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return Result.Success();
    }
}
