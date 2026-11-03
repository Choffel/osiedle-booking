using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Domain.Tests.Entities;

public class BookableServiceTests
{
    [Fact]
    public void Create_WithValidData_InitializesActiveService()
    {
        var employeeId = EmployeeId.New();
        var before = DateTime.UtcNow;

        var service = BookableService.Create(employeeId, "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));

        Assert.NotEqual(Guid.Empty, service.Id.Value);
        Assert.Equal(employeeId, service.EmployeeId);
        Assert.Equal("Haircut", service.ServiceName);
        Assert.Equal("Classic haircut", service.Description);
        Assert.Equal(50m, service.Price);
        Assert.Equal(TimeSpan.FromMinutes(45), service.Duration);
        Assert.Equal(ServiceStatus.Active, service.Status);
        Assert.InRange(service.CreatedAt, before, DateTime.UtcNow);
        Assert.Null(service.UpdatedAt);
    }

    [Fact]
    public void UpdateInfo_UpdatesFieldsAndSetsUpdatedAt()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));

        service.UpdateInfo("Beard trim", "Beard shaping", 30m, TimeSpan.FromMinutes(20));

        Assert.Equal("Beard trim", service.ServiceName);
        Assert.Equal("Beard shaping", service.Description);
        Assert.Equal(30m, service.Price);
        Assert.Equal(TimeSpan.FromMinutes(20), service.Duration);
        Assert.NotNull(service.UpdatedAt);
    }

    [Theory]
    [InlineData("", "Description", 50, 30)]
    [InlineData("Haircut", "Description", 0, 30)]
    [InlineData("Haircut", "Description", -5, 30)]
    [InlineData("Haircut", "Description", 50, 0)]
    [InlineData("Haircut", "Description", 50, -10)]
    public void Create_WithInvalidData_Throws(string name, string description, decimal price, int durationMinutes)
    {
        Assert.Throws<ArgumentException>(() => BookableService.Create(
            EmployeeId.New(), name, description, price, TimeSpan.FromMinutes(durationMinutes)));
    }

    [Theory]
    [InlineData("", "Description", 50, 30)]
    [InlineData("Haircut", "Description", 0, 30)]
    [InlineData("Haircut", "Description", 50, 0)]
    public void UpdateInfo_WithInvalidData_Throws(string name, string description, decimal price, int durationMinutes)
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));

        Assert.Throws<ArgumentException>(() => service.UpdateInfo(
            name, description, price, TimeSpan.FromMinutes(durationMinutes)));
    }

    [Fact]
    public void DeactivateService_SetsInactiveStatusAndIsIdempotent()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));

        service.DeactivateService();
        var firstUpdatedAt = service.UpdatedAt;
        service.DeactivateService();

        Assert.Equal(ServiceStatus.Inactive, service.Status);
        Assert.Equal(firstUpdatedAt, service.UpdatedAt);
    }

    [Fact]
    public void ActivateService_SetsActiveStatus()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));
        service.DeactivateService();

        service.ActivateService();

        Assert.Equal(ServiceStatus.Active, service.Status);
        Assert.NotNull(service.UpdatedAt);
    }

    [Fact]
    public void ActivateService_WhenAlreadyActive_DoesNotChangeUpdatedAt()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));

        service.ActivateService();

        Assert.Equal(ServiceStatus.Active, service.Status);
        Assert.Null(service.UpdatedAt);
    }

    [Fact]
    public void ArchiveService_SetsArchivedStatus()
    {
        var service = BookableService.Create(
            EmployeeId.New(), "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));

        service.ArchiveService();

        Assert.Equal(ServiceStatus.Archived, service.Status);
        Assert.NotNull(service.UpdatedAt);
    }
}
