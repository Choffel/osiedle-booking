using Business.Application.DTOs.BusinessDTOs;
using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Employee;
using Business.Domain.Errors;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.BusinessServices;

public class BusinessInternalApi : IBusinessInternalApi
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IEmployeeWorkingHoursRepository _workingHoursRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IBookableServiceRepository _bookableServiceRepository;

    public BusinessInternalApi(
        IBusinessRepository businessRepository,
        IEmployeeWorkingHoursRepository workingHoursRepository,
        IEmployeeRepository employeeRepository,
        IBookableServiceRepository bookableServiceRepository)
    {
        _businessRepository = businessRepository;
        _workingHoursRepository = workingHoursRepository;
        _employeeRepository = employeeRepository;
        _bookableServiceRepository = bookableServiceRepository;
    }


    public async Task<Result<BusinessShortInfoDto>> GetBusinessInfoAsync(BusinessId businessId, CancellationToken ct = default)
    {
        var businessInfo = await _businessRepository.GetShortInfoAsync(businessId, ct);
        
        if (businessInfo is null)
        {
            return Result.Failure<BusinessShortInfoDto>(BusinessErrors.NotFound);
        }

        return Result.Success(businessInfo);
    }

    public Task<bool> EmployeeBelongsToBusinessAsync(EmployeeId employeeId, BusinessId businessId, CancellationToken ct = default)
    {
        return _businessRepository.EmployeeBelongsToBusinessAsync(employeeId, businessId, ct);
    }

    public Task<bool> ServiceBelongsToBusinessAsync(BookableServiceId serviceId, BusinessId businessId, CancellationToken ct = default)
    {
        return _businessRepository.BookableServiceBelongsToBusinessAsync(serviceId, businessId, ct);
    }

    public Task<bool> ServiceBelongsToEmployeeAsync(BookableServiceId serviceId, EmployeeId employeeId, CancellationToken ct = default)
    {
        return _businessRepository.BookableServiceBelongsToEmployeeAsync(serviceId, employeeId, ct);
    }

    public async Task<Result<IReadOnlyCollection<WorkingHoursDto>>> GetWorkingHoursForRangeAsync(EmployeeId employeeId, DateOnly startDate, DateOnly endDate,
        CancellationToken ct = default)
    {
        if(endDate <= startDate)
        {
            return Result.Failure<IReadOnlyCollection<WorkingHoursDto>>(EmployeeErrors.InvalidDateRange);
        }
        
        bool employeeExists = await _employeeRepository.ExistsEmployeeAsync(employeeId, ct);
        
        if(!employeeExists)
        {
            return Result.Failure<IReadOnlyCollection<WorkingHoursDto>>(EmployeeErrors.NotFound);
        }
        
        var workingHours = await _workingHoursRepository.GetWorkingHoursForRangeAsync(employeeId, startDate, endDate, ct);


        return Result.Success(workingHours);
    }
}