using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.Employee;
using Business.Application.Services.EmployeeServices;
using Business.Domain.Entities;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using EmployeeAggregate = Business.Domain.Aggregates.Employee;

namespace Business.Application.Tests.InternalApis;

public class EmployeeInternalApiTests
{
    private static readonly DateOnly Date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IEmployeeWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<IEmployeeReadOnlyRepository> _employeeReadOnlyRepository = new();
    private readonly EmployeeInternalApi _sut;

    public EmployeeInternalApiTests()
    {
        _sut = new EmployeeInternalApi(
            _employeeRepository.Object,
            _workingHoursRepository.Object,
            _employeeReadOnlyRepository.Object);
    }

    [Fact]
    public async Task GetShortInfoAsync_WhenEmployeeExists_ReturnsShortInfo()
    {
        var shortInfo = new EmployeeShortInfoDto(Guid.NewGuid(), "Jan", "Kowalski", "Barber");
        _employeeReadOnlyRepository
            .Setup(r => r.GetShortInfoAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortInfo);

        var result = await _sut.GetShortInfoAsync(EmployeeId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(shortInfo, result.Value);
    }

    [Fact]
    public async Task GetShortInfoAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        _employeeReadOnlyRepository
            .Setup(r => r.GetShortInfoAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeShortInfoDto?)null);

        var result = await _sut.GetShortInfoAsync(EmployeeId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task IsAvailableForBookingAsync_WhenEmployeeMissing_ReturnsFalse()
    {
        _employeeRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeAggregate?)null);

        var result = await _sut.IsAvailableForBookingAsync(EmployeeId.New());

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task IsAvailableForBookingAsync_WhenEmployeeIsActive_ReturnsTrue()
    {
        var employee = CreateEmployee();
        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.IsAvailableForBookingAsync(employee.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task IsAvailableForBookingAsync_WhenEmployeeIsArchived_ReturnsFalse()
    {
        var employee = CreateEmployee();
        employee.ArchiveEmployee();
        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.IsAvailableForBookingAsync(employee.Id);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task GetWorkingHoursAsync_WhenNoEntry_ReturnsNotFound()
    {
        _workingHoursRepository
            .Setup(r => r.GetByDateAsync(
                It.IsAny<EmployeeId>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkingHours?)null);

        var result = await _sut.GetWorkingHoursAsync(EmployeeId.New(), Date);

        Assert.True(result.IsFailure);
        Assert.Equal(WorkingHoursErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetWorkingHoursAsync_WhenEntryExists_ReturnsWorkingHours()
    {
        var employeeId = EmployeeId.New();
        var workingHours = WorkingHours.Create(
            employeeId, Date, TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(17, 0)));

        _workingHoursRepository
            .Setup(r => r.GetByDateAsync(
                It.IsAny<EmployeeId>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        var result = await _sut.GetWorkingHoursAsync(employeeId, Date);

        Assert.True(result.IsSuccess);
        Assert.Equal(Date, result.Value.Date);
        Assert.Equal(new TimeOnly(9, 0), result.Value.StartTime);
        Assert.Equal(new TimeOnly(17, 0), result.Value.EndTime);
    }

    private static EmployeeAggregate CreateEmployee()
    {
        return EmployeeAggregate.Create(
            null,
            EmployeeName.Create("Jan", "Kowalski"),
            "Barber",
            "Great",
            "jan@example.com");
    }
}
