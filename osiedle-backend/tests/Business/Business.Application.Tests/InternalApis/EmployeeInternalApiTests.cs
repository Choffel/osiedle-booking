using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces.Employee;
using Business.Application.Services.EmployeeServices;
using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;

namespace Business.Application.Tests.InternalApis;

public class EmployeeInternalApiTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IEmployeeWorkingHoursRepository> _workingHoursRepository = new();
    private readonly EmployeeInternalApi _sut;

    public EmployeeInternalApiTests()
    {
        _sut = new EmployeeInternalApi(_employeeRepository.Object, _workingHoursRepository.Object);
    }

    [Fact]
    public async Task ExistsEmployeeAsync_DelegatesToRepository()
    {
        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.ExistsEmployeeAsync(EmployeeId.New());

        Assert.True(result);
        _employeeRepository.Verify(
            r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_WhenEmployeeExists_ReturnsStatus()
    {
        var status = new EmployeeStatusDto(EmploymentStatus.OnLeave, Guid.NewGuid());
        _employeeRepository
            .Setup(r => r.GetStatusAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        var result = await _sut.GetStatusAsync(EmployeeId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(status, result.Value);
    }

    [Fact]
    public async Task GetStatusAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(r => r.GetStatusAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeStatusDto?)null);

        var result = await _sut.GetStatusAsync(EmployeeId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetShortInfoAsync_WhenEmployeeExists_ReturnsShortInfo()
    {
        var shortInfo = new EmployeeShortInfoDto(Guid.NewGuid(), "Jan", "Kowalski", "Barber");
        _employeeRepository
            .Setup(r => r.GetShortInfoAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortInfo);

        var result = await _sut.GetShortInfoAsync(EmployeeId.New());

        Assert.True(result.IsSuccess);
        Assert.Equal(shortInfo, result.Value);
    }

    [Fact]
    public async Task GetShortInfoAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(r => r.GetShortInfoAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeShortInfoDto?)null);

        var result = await _sut.GetShortInfoAsync(EmployeeId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetWorkingHoursAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetWorkingHoursAsync(EmployeeId.New(), new DateOnly(2026, 10, 23));

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetWorkingHoursAsync_WhenNoEntry_ReturnsEmptyCollection()
    {
        var employeeId = EmployeeId.New();
        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workingHoursRepository
            .Setup(r => r.GetWorkingHoursByDateAsync(
                It.IsAny<EmployeeId>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkingHours?)null);

        var result = await _sut.GetWorkingHoursAsync(employeeId, new DateOnly(2026, 10, 23));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetWorkingHoursAsync_WhenEntryExists_ReturnsSingleWorkingHours()
    {
        var date = new DateOnly(2026, 10, 23);
        var employee = Business.Domain.Aggregates.Employee.Create(
            null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");
        var workingHours = WorkingHours.Create(employee.Id, date, new TimeOnly(9, 0), new TimeOnly(17, 0));

        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workingHoursRepository
            .Setup(r => r.GetWorkingHoursByDateAsync(
                It.IsAny<EmployeeId>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        var result = await _sut.GetWorkingHoursAsync(employee.Id, date);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value);
        Assert.Equal(date, item.Date);
        Assert.Equal(new TimeOnly(9, 0), item.StartTime);
        Assert.Equal(new TimeOnly(17, 0), item.EndTime);
    }
}
