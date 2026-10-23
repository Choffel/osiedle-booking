using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Employee;
using Business.Application.Services.EmployeeServices;
using Business.Domain.Entities;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using EmployeeAggregate = Business.Domain.Aggregates.Employee;

namespace Business.Application.Tests.Services;

public class EmployeeWorkingHoursServiceTests
{
    private static readonly DateOnly Date = new(2026, 10, 23);

    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IEmployeeWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly EmployeeWorkingHoursService _sut;

    public EmployeeWorkingHoursServiceTests()
    {
        _sut = new EmployeeWorkingHoursService(
            _employeeRepository.Object,
            _workingHoursRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task SetWorkingHoursAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        var employeeId = Guid.NewGuid();
        var dto = new SetWorkingHoursDto(Date, new TimeOnly(9, 0), new TimeOnly(17, 0));
        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeAggregate?)null);

        var result = await _sut.SetWorkingHoursAsync(employeeId, dto);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetWorkingHoursAsync_WhenNoEntryForDate_AddsWorkingHoursAndSaves()
    {
        var employee = EmployeeAggregate.Create(
            null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");
        var dto = new SetWorkingHoursDto(Date, new TimeOnly(9, 0), new TimeOnly(17, 0));
        WorkingHours? added = null;

        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _workingHoursRepository
            .Setup(r => r.GetWorkingHoursByDateAsync(It.IsAny<EmployeeId>(), Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkingHours?)null);
        _workingHoursRepository
            .Setup(r => r.AddWorkingHours(It.IsAny<WorkingHours>()))
            .Callback<WorkingHours>(workingHours => added = workingHours);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.SetWorkingHoursAsync(employee.Id.Value, dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(employee.Id, added!.EmployeeId);
        Assert.Equal(Date, added.Date);
        Assert.Equal(dto.StartTime, added.StartTime);
        Assert.Equal(dto.EndTime, added.EndTime);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetWorkingHoursAsync_WhenEntryExists_UpdatesWorkingHoursAndSaves()
    {
        var employee = EmployeeAggregate.Create(
            null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");
        var existing = WorkingHours.Create(employee.Id, Date, new TimeOnly(8, 0), new TimeOnly(16, 0));
        var dto = new SetWorkingHoursDto(Date, new TimeOnly(10, 0), new TimeOnly(18, 0));

        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _workingHoursRepository
            .Setup(r => r.GetWorkingHoursByDateAsync(It.IsAny<EmployeeId>(), Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.SetWorkingHoursAsync(employee.Id.Value, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(dto.StartTime, existing.StartTime);
        Assert.Equal(dto.EndTime, existing.EndTime);
        _workingHoursRepository.Verify(r => r.AddWorkingHours(It.IsAny<WorkingHours>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearWorkingHoursAsync_WhenEntryExists_RemovesAndSaves()
    {
        var employee = EmployeeAggregate.Create(
            null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");
        var existing = WorkingHours.Create(employee.Id, Date, new TimeOnly(9, 0), new TimeOnly(17, 0));

        _workingHoursRepository
            .Setup(r => r.GetWorkingHoursByDateAsync(It.IsAny<EmployeeId>(), Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _workingHoursRepository
            .Setup(r => r.RemoveWorkingHours(It.IsAny<WorkingHours>()));
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ClearWorkingHoursAsync(employee.Id.Value, Date);

        Assert.True(result.IsSuccess);
        _workingHoursRepository.Verify(r => r.RemoveWorkingHours(existing), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearWorkingHoursAsync_WhenEntryMissing_DoesNotSave()
    {
        _workingHoursRepository
            .Setup(r => r.GetWorkingHoursByDateAsync(It.IsAny<EmployeeId>(), Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkingHours?)null);

        var result = await _sut.ClearWorkingHoursAsync(Guid.NewGuid(), Date);

        Assert.True(result.IsSuccess);
        _workingHoursRepository.Verify(r => r.RemoveWorkingHours(It.IsAny<WorkingHours>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
