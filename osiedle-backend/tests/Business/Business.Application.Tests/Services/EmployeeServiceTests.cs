using Business.Application.DTOs.EmployeeDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Employee;
using Business.Application.Services.EmployeeServices;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObjects;
using EmployeeAggregate = Business.Domain.Aggregates.Employee;

namespace Business.Application.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _sut = new EmployeeService(
            _employeeRepository.Object,
            _businessRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithNewEmail_AddsNormalizedEmployeeAndSaves()
    {
        var dto = new EmployeeCreateDto(
            null,
            "Jan",
            "Kowalski",
            "Barber",
            "Great",
            "  Jan.Kowalski@Example.COM  ");
        EmployeeAggregate? added = null;

        _employeeRepository
            .Setup(r => r.AddEmployee(It.IsAny<EmployeeAggregate>()))
            .Callback<EmployeeAggregate>(employee => added = employee);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateEmployeeAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal("Jan", added!.EmployeeName.FirstName);
        Assert.Equal("Kowalski", added.EmployeeName.LastName);
        Assert.Equal(dto.Position, added.Position);
        Assert.Equal(dto.Review, added.Review);
        Assert.Equal("jan.kowalski@example.com", added.Email);
        Assert.Equal(added.Id, result.Value);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithBusinessId_PassesBusinessIdToAggregate()
    {
        var businessId = BusinessId.New();
        var dto = new EmployeeCreateDto(
            businessId.Value,
            "Jan",
            "Kowalski",
            "Barber",
            "Great",
            "jan@example.com");
        EmployeeAggregate? added = null;

        _businessRepository
            .Setup(r => r.ExistsByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _employeeRepository
            .Setup(r => r.AddEmployee(It.IsAny<EmployeeAggregate>()))
            .Callback<EmployeeAggregate>(employee => added = employee);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.CreateEmployeeAsync(dto);

        Assert.NotNull(added);
        Assert.Equal(businessId, added!.BusinessId);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithUnknownBusinessId_ReturnsValidationError()
    {
        var businessId = BusinessId.New();
        var dto = new EmployeeCreateDto(
            businessId.Value,
            "Jan",
            "Kowalski",
            "Barber",
            "Great",
            "jan@example.com");

        _businessRepository
            .Setup(r => r.ExistsByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.CreateEmployeeAsync(dto);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.BusinessNotFound, result.Error);
        _employeeRepository.Verify(r => r.AddEmployee(It.IsAny<EmployeeAggregate>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithoutReview_CreatesEmployeeWithNullReview()
    {
        var dto = new EmployeeCreateDto(
            null,
            "Jan",
            "Kowalski",
            "Barber",
            null,
            "jan@example.com");
        EmployeeAggregate? added = null;

        _employeeRepository
            .Setup(r => r.AddEmployee(It.IsAny<EmployeeAggregate>()))
            .Callback<EmployeeAggregate>(employee => added = employee);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateEmployeeAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Null(added!.Review);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenEmailBelongsToAnotherEmployee_ReturnsConflict()
    {
        var employee = CreateEmployee();
        var dto = new UpdateEmployeeProfileDto("Jan", "Kowalski", "Barber", "Great", "taken@example.com");

        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _employeeRepository
            .Setup(r => r.ExistsByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateProfileAsync(employee.Id, dto);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.AlreadyExists, result.Error);
        Assert.Equal("jan@example.com", employee.Email);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenEmailIsUnchanged_DoesNotCheckEmailAvailability()
    {
        var employee = CreateEmployee();
        var dto = new UpdateEmployeeProfileDto("Jan", "Kowalski", "Barber", "Great", "JAN@Example.COM");

        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateProfileAsync(employee.Id, dto);

        Assert.True(result.IsSuccess);
        _employeeRepository.Verify(
            r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenEmployeeExists_UpdatesFieldsAndSaves()
    {
        var employee = CreateEmployee();
        var dto = new UpdateEmployeeProfileDto("Adam", "Nowak", "Stylist", "Very good", "adam@example.com");

        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateProfileAsync(employee.Id, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(dto.FirstName, employee.EmployeeName.FirstName);
        Assert.Equal(dto.LastName, employee.EmployeeName.LastName);
        Assert.Equal(dto.Position, employee.Position);
        Assert.Equal(dto.Review, employee.Review);
        Assert.NotNull(employee.UpdatedAt);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeAggregate?)null);

        var result = await _sut.UpdateProfileAsync(EmployeeId.New(),
            new UpdateEmployeeProfileDto("Adam", "Nowak", "Stylist", "Very good", "adam@example.com"));

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_WhenEmployeeExists_ArchivesEmployeeAndSaves()
    {
        var employee = CreateEmployee();

        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeactivateEmployeeAsync(employee.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(EmploymentStatus.Archived, employee.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_WhenEmployeeMissing_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(r => r.GetEmployeeByIdAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeAggregate?)null);

        var result = await _sut.DeactivateEmployeeAsync(EmployeeId.New());

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
