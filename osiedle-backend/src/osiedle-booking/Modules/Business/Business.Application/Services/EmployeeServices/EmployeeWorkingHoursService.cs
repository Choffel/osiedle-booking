using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Employee;
using Business.Domain.Entities;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Application.Services.EmployeeServices;

public class EmployeeWorkingHoursService : IEmployeeWorkingHoursService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IEmployeeWorkingHoursRepository _employeeWorkingHoursRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeWorkingHoursService(
        IEmployeeRepository employeeRepository,
        IEmployeeWorkingHoursRepository employeeWorkingHoursRepository,
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _employeeWorkingHoursRepository = employeeWorkingHoursRepository;
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> SetWorkingHoursAsync(UserId currentUserId, EmployeeId employeeId, SetWorkingHoursDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var canManage = await CanManageAsync(employee, currentUserId, ct);
        if (!canManage)
            return Result.Failure(EmployeeErrors.Forbidden);

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

    public async Task<Result> ClearWorkingHoursAsync(UserId currentUserId, EmployeeId employeeId, DateOnly date, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        if (employee is null)
            return Result.Failure(EmployeeErrors.NotFound);

        var canManage = await CanManageAsync(employee, currentUserId, ct);
        if (!canManage)
            return Result.Failure(EmployeeErrors.Forbidden);

        var workingHours = await _employeeWorkingHoursRepository.GetWorkingHoursByDateAsync(employeeId, date, ct);
        
        if (workingHours != null)
        {
            _employeeWorkingHoursRepository.RemoveWorkingHours(workingHours);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return Result.Success();
    }

    private async Task<bool> CanManageAsync(Business.Domain.Aggregates.Employee employee, UserId currentUserId, CancellationToken ct)
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
