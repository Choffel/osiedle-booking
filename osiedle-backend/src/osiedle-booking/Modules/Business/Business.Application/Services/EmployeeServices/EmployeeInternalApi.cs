using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.DTOs.WorkingHoursDTOs;
using Business.Application.Interfaces.Employee;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.EmployeeServices;

public class EmployeeInternalApi : IEmployeeInternalApi
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeWorkingHoursRepository _employeeWorkingHoursRepository;
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;

    public EmployeeInternalApi(IEmployeeRepository employeeRepository,
        IEmployeeWorkingHoursRepository employeeWorkingHoursRepository
        , IEmployeeReadOnlyRepository employeeReadOnlyRepository)
    {
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _employeeRepository = employeeRepository;
        _employeeWorkingHoursRepository = employeeWorkingHoursRepository;
    }
    
    public async Task<Result<EmployeeShortInfoDto>> GetShortInfoAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        var shortInfo = await _employeeReadOnlyRepository.GetShortInfoAsync(employeeId, ct);

        if (shortInfo == null)
        {
            return Result.Failure<EmployeeShortInfoDto>(EmployeeErrors.NotFound);
        }

        return Result.Success(shortInfo);
    }
    
    public async Task<bool> IsAvailableForBookingAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return false;
        }
        return employee.CanBeBooked();
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
        
        var workingHours = await _employeeWorkingHoursRepository.GetByDateAsync(employeeId, date, ct);
        
        if (workingHours is null)
        {
            return Result.Success<IReadOnlyCollection<WorkingHoursDto>>([]);
        }
        
        IReadOnlyCollection<WorkingHoursDto> result =
        [
            new WorkingHoursDto(
                workingHours.Date, 
                workingHours.Slot)
        ];

        return Result.Success(result);
    }
}
