using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.Employee;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.EmployeeServices;

public class EmployeeInternalApi : IEmployeeInternalApi
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeWorkingHoursRepository _employeeWorkingHoursRepository;

    public EmployeeInternalApi(IEmployeeRepository employeeRepository, IEmployeeWorkingHoursRepository employeeWorkingHoursRepository)
    {
        _employeeRepository = employeeRepository;
        _employeeWorkingHoursRepository = employeeWorkingHoursRepository;
    }

    public Task<bool> ExistsEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        return _employeeRepository.ExistsEmployeeAsync(employeeId, ct);
    }

    public async Task<Result<EmployeeStatusDto>> GetStatusAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        var status = await _employeeRepository.GetStatusAsync(employeeId, ct);

        if (status == null)
        {
            return Result.Failure<EmployeeStatusDto>(EmployeeErrors.NotFound);
        }

        return Result.Success(status);
    }

    public async Task<Result<EmployeeShortInfoDto>> GetShortInfoAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        var shortInfo = await _employeeRepository.GetShortInfoAsync(employeeId, ct);

        if (shortInfo == null)
        {
            return Result.Failure<EmployeeShortInfoDto>(EmployeeErrors.NotFound);
        }

        return Result.Success(shortInfo);
    }
    
    public async Task<Result<IReadOnlyCollection<WorkingHoursDto>>> GetWorkingHoursAsync(
        EmployeeId employeeId, 
        DateOnly date, 
        CancellationToken ct = default)
    {
        var employeeExists = await _employeeRepository.ExistsEmployeeAsync(employeeId, ct);
        if (!employeeExists)
        {
            return Result.Failure<IReadOnlyCollection<WorkingHoursDto>>(EmployeeErrors.NotFound);
        }
        
        var workingHours = await _employeeWorkingHoursRepository.GetWorkingHoursByDateAsync(employeeId, date, ct);
        
        if (workingHours is null)
        {
            return Result.Success<IReadOnlyCollection<WorkingHoursDto>>([]);
        }
        
        IReadOnlyCollection<WorkingHoursDto> result =
        [
            new WorkingHoursDto(
                workingHours.Date, 
                workingHours.StartTime, 
                workingHours.EndTime)
        ];

        return Result.Success(result);
    }
}
