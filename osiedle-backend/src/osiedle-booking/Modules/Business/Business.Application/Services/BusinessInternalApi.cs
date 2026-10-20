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


namespace Business.Application.Services;

public class BusinessInternalApi : IBusinessInternalApi
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IEmployeeWorkingHoursRepository _workingHoursRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IBookableServiceRepository _bookableServiceRepository;

    public BusinessInternalApi(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
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
        
    }

    public Task<bool> ServiceBelongsToBusinessAsync(BookableServiceId serviceId, BusinessId businessId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ServiceBelongsToEmployeeAsync(BookableServiceId serviceId, EmployeeId employeeId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyCollection<WorkingHoursDto>>> GetWorkingHoursForRangeAsync(EmployeeId employeeId, DateOnly startDate, DateOnly endDate,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}