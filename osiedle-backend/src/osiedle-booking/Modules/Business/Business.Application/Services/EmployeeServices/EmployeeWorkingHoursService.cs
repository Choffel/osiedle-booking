using Business.Application.DTOs.WorkingHoursDTOs;
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

    public async Task<Result> SetWorkingHoursAsync(Guid employeeId,
        SetWorkingHoursDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(EmployeeId.From(employeeId), ct);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var workingHours = await _employeeWorkingHoursRepository.GetByDateAsync(EmployeeId.From(employeeId), dto.Date, ct);
        var slot = TimeSlot.Create(dto.StartTime, dto.EndTime);

        if (workingHours is null)
        {
            var newWorkingHours = WorkingHours.Create(EmployeeId.From(employeeId), dto.Date, slot);
            _employeeWorkingHoursRepository.Add(newWorkingHours);
        }
        else
        {
            workingHours.UpdateSlot(slot);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ClearWorkingHoursAsync(Guid employeeId, DateOnly date, CancellationToken ct = default)
    {
        var workingHours = await _employeeWorkingHoursRepository.GetByDateAsync(EmployeeId.From(employeeId), date, ct);
        
        if (workingHours != null)
        {
            _employeeWorkingHoursRepository.Remove(workingHours);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return Result.Success();
    }
}
