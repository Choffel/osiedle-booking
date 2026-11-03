using Business.Application.DTOs.BusinessDTOs;
using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Employee;
using Business.Application.Services.BusinessServices;
using Business.Domain.Errors;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Application.Tests.InternalApis;

public class BusinessInternalApiTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IEmployeeWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IBookableServiceRepository> _bookableServiceRepository = new();
    private readonly BusinessInternalApi _sut;

    public BusinessInternalApiTests()
    {
        _sut = new BusinessInternalApi(
            _businessRepository.Object,
            _workingHoursRepository.Object,
            _employeeRepository.Object,
            _bookableServiceRepository.Object);
    }

    [Fact]
    public async Task GetBusinessInfoAsync_WhenBusinessExists_ReturnsShortInfo()
    {
        var shortInfo = new BusinessShortInfoDto(
            Guid.NewGuid(), Guid.NewGuid(), "Beauty Salon", "Description", "Location");
        _businessRepository
            .Setup(r => r.GetShortInfoAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortInfo);

        var result = await _sut.GetBusinessInfoAsync(BusinessId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(shortInfo, result.Value);
    }

    [Fact]
    public async Task GetBusinessInfoAsync_WhenBusinessMissing_ReturnsNotFound()
    {
        _businessRepository
            .Setup(r => r.GetShortInfoAsync(It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessShortInfoDto?)null);

        var result = await _sut.GetBusinessInfoAsync(BusinessId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(BusinessErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task EmployeeBelongsToBusinessAsync_DelegatesToRepository()
    {
        _businessRepository
            .Setup(r => r.EmployeeBelongsToBusinessAsync(
                It.IsAny<EmployeeId>(), It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.EmployeeBelongsToBusinessAsync(EmployeeId.New(), BusinessId.New());

        Assert.True(result);
        _businessRepository.Verify(
            r => r.EmployeeBelongsToBusinessAsync(
                It.IsAny<EmployeeId>(), It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ServiceBelongsToBusinessAsync_DelegatesToRepository()
    {
        _businessRepository
            .Setup(r => r.BookableServiceBelongsToBusinessAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<BusinessId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.ServiceBelongsToBusinessAsync(BookableServiceId.New(), BusinessId.New());

        Assert.False(result);
    }

    [Fact]
    public async Task ServiceBelongsToEmployeeAsync_DelegatesToRepository()
    {
        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.ServiceBelongsToEmployeeAsync(BookableServiceId.New(), EmployeeId.New());

        Assert.True(result);
        _bookableServiceRepository.Verify(
            r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWorkingHoursForRangeAsync_WithInvalidRange_ReturnsValidationError()
    {
        var start = new DateOnly(2026, 10, 23);
        var end = new DateOnly(2026, 10, 23);

        var result = await _sut.GetWorkingHoursForRangeAsync(EmployeeId.New(), start, end);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.InvalidDateRange, result.Error);
        _employeeRepository.Verify(
            r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWorkingHoursForRangeAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetWorkingHoursForRangeAsync(
            EmployeeId.New(), new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 25));

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
        _workingHoursRepository.Verify(
            r => r.GetWorkingHoursForRangeAsync(
                It.IsAny<EmployeeId>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWorkingHoursForRangeAsync_WithValidRequest_ReturnsWorkingHours()
    {
        var workingHours = new List<WorkingHoursDto>
        {
            new(new DateOnly(2026, 10, 21), new TimeOnly(9, 0), new TimeOnly(17, 0))
        };

        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workingHoursRepository
            .Setup(r => r.GetWorkingHoursForRangeAsync(
                It.IsAny<EmployeeId>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        var result = await _sut.GetWorkingHoursForRangeAsync(
            EmployeeId.New(), new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 25));

        Assert.True(result.IsSuccess);
        Assert.Equal(workingHours, result.Value);
    }
}
