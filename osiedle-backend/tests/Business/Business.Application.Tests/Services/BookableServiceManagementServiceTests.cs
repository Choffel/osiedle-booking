using Business.Application.DTOs.BookableServiceDTOs;
using Business.Application.Interfaces;
using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Employee;
using Business.Application.Services.BookableItemServices;
using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.Errors;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Application.Tests.Services;

public class BookableServiceManagementServiceTests
{
    private readonly Mock<IBookableServiceRepository> _bookableServiceRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly BookableServiceManagementService _sut;

    public BookableServiceManagementServiceTests()
    {
        _sut = new BookableServiceManagementService(
            _bookableServiceRepository.Object,
            _employeeRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateBookableServiceAsync_WhenEmployeeMissing_ReturnsEmployeeNotFound()
    {
        var dto = new CreateBookableServiceDto(Guid.NewGuid(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.CreateBookableServiceAsync(dto, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.EmployeeNotFound, result.Error);
        _bookableServiceRepository.Verify(r => r.Add(It.IsAny<BookableService>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookableServiceAsync_WhenNameAlreadyTaken_ReturnsConflict()
    {
        var dto = new CreateBookableServiceDto(Guid.NewGuid(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.ExistsByNameForEmployeeAsync(
                dto.ServiceName, It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateBookableServiceAsync(dto, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NameAlreadyTaken, result.Error);
        _bookableServiceRepository.Verify(r => r.Add(It.IsAny<BookableService>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookableServiceAsync_WithValidData_AddsServiceAndSaves()
    {
        var dto = new CreateBookableServiceDto(Guid.NewGuid(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        BookableService? added = null;

        _employeeRepository
            .Setup(r => r.ExistsEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.ExistsByNameForEmployeeAsync(
                dto.ServiceName, It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _bookableServiceRepository
            .Setup(r => r.Add(It.IsAny<BookableService>(), It.IsAny<CancellationToken>()))
            .Callback<BookableService, CancellationToken>((service, _) => added = service);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateBookableServiceAsync(dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(dto.ServiceName, added!.ServiceName);
        Assert.Equal(dto.ServiceDescription, added.Description);
        Assert.Equal(dto.ServicePrice, added.Price);
        Assert.Equal(dto.ServiceDuration, added.Duration);
        Assert.Equal(ServiceStatus.Active, added.Status);
        Assert.Equal(dto.EmployeeId, result.Value.EmployeeId);
        Assert.Equal(added.Id.Value, result.Value.BookableServiceId);
        Assert.Equal(dto.ServiceName, result.Value.ServiceName);
        Assert.Equal(dto.ServicePrice, result.Value.ServicePrice);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookableServiceAsync_WhenServiceDoesNotBelongToEmployee_ReturnsAccessDenied()
    {
        var dto = new UpdateBookableServiceDto(
            Guid.NewGuid(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.UpdateBookableServiceAsync(Guid.NewGuid(), dto, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NotFoundOrAccessDenied, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBookableServiceAsync_WhenServiceMissing_ReturnsNotFound()
    {
        var dto = new UpdateBookableServiceDto(
            Guid.NewGuid(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookableService?)null);

        var result = await _sut.UpdateBookableServiceAsync(Guid.NewGuid(), dto, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NotFound, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBookableServiceAsync_WhenNewNameIsTaken_ReturnsConflict()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        var dto = new UpdateBookableServiceDto(
            Guid.NewGuid(), "Beard trim", "New description", 60m, TimeSpan.FromMinutes(30));

        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _bookableServiceRepository
            .Setup(r => r.ExistsByNameForEmployeeAsync(
                dto.ServiceName, It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateBookableServiceAsync(service.Id.Value, dto, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NameAlreadyTaken, result.Error);
        Assert.Equal("Haircut", service.ServiceName);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBookableServiceAsync_WithNewAvailableName_UpdatesServiceAndSaves()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        var dto = new UpdateBookableServiceDto(
            Guid.NewGuid(), "Beard trim", "New description", 60m, TimeSpan.FromMinutes(30));

        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _bookableServiceRepository
            .Setup(r => r.ExistsByNameForEmployeeAsync(
                dto.ServiceName, It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateBookableServiceAsync(service.Id.Value, dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(dto.ServiceName, service.ServiceName);
        Assert.Equal(dto.ServiceDescription, service.Description);
        Assert.Equal(dto.ServicePrice, service.Price);
        Assert.Equal(dto.ServiceDuration, service.Duration);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookableServiceAsync_WhenNameIsUnchanged_DoesNotCheckNameAvailability()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        var dto = new UpdateBookableServiceDto(
            Guid.NewGuid(), "Haircut", "New description", 60m, TimeSpan.FromMinutes(30));

        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateBookableServiceAsync(service.Id.Value, dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _bookableServiceRepository.Verify(
            r => r.ExistsByNameForEmployeeAsync(
                It.IsAny<string>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBookableServiceAsync_WhenServiceMissing_ReturnsNotFound()
    {
        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookableService?)null);

        var result = await _sut.DeleteBookableServiceAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteBookableServiceAsync_WhenAlreadyArchived_ReturnsConflict()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        service.ArchiveService();

        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var result = await _sut.DeleteBookableServiceAsync(service.Id.Value, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.AlreadyArchived, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBookableServiceAsync_WithActiveService_ArchivesAndSaves()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));

        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeleteBookableServiceAsync(service.Id.Value, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceStatus.Archived, service.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateBookableServiceAsync_WhenServiceDoesNotBelongToEmployee_ReturnsAccessDenied()
    {
        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.ActivateBookableServiceAsync(
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NotFoundOrAccessDenied, result.Error);
    }

    [Fact]
    public async Task ActivateBookableServiceAsync_WhenServiceInactive_ActivatesAndSaves()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));
        service.DeactivateService();

        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ActivateBookableServiceAsync(
            Guid.NewGuid(), service.Id.Value, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceStatus.Active, service.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateBookableServiceAsync_WhenServiceDoesNotBelongToEmployee_ReturnsAccessDenied()
    {
        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.DeactivateBookableServiceAsync(
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BookableServiceErrors.NotFoundOrAccessDenied, result.Error);
    }

    [Fact]
    public async Task DeactivateBookableServiceAsync_WhenServiceActive_DeactivatesAndSaves()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(45));

        _bookableServiceRepository
            .Setup(r => r.BelongsToEmployeeAsync(
                It.IsAny<BookableServiceId>(), It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _bookableServiceRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<BookableServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeactivateBookableServiceAsync(
            Guid.NewGuid(), service.Id.Value, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceStatus.Inactive, service.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
