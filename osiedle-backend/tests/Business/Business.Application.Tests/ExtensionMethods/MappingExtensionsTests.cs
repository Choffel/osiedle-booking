using Business.Application.ExtensionMethods;
using Business.Domain.Enums;
using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;
using BusinessServiceEntity = Business.Domain.Entities.BookableService;
using EmployeeAggregate = Business.Domain.Aggregates.Employee;

namespace Business.Application.Tests.ExtensionMethods;

public class MappingExtensionsTests
{
    [Fact]
    public void ToResponseDto_MapsBusinessFields()
    {
        var ownerId = UserId.New();
        var business = BusinessAggregate.Create("Beauty Salon", ownerId, "Description", "Location");

        var dto = business.ToResponseDto();

        Assert.Equal(business.Id.Value, dto.Id);
        Assert.Equal(ownerId.Value, dto.OwnerId);
        Assert.Equal(business.Name, dto.Name);
        Assert.Equal(business.Description, dto.Description);
        Assert.Equal(business.Location, dto.Location);
        Assert.Equal(nameof(BusinessStatus.Active), dto.Status);
        Assert.Equal(business.CreatedAt, dto.CreatedAt);
        Assert.Equal(business.UpdatedAt, dto.UpdatedAt);
    }

    [Fact]
    public void ToResponseDto_MapsEmployeeFields()
    {
        var businessId = BusinessId.New();
        var employee = EmployeeAggregate.Create(
            businessId, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");

        var dto = employee.ToResponseDto();

        Assert.Equal(employee.Id.Value, dto.Id);
        Assert.Equal(businessId.Value, dto.BusinessId);
        Assert.Equal(employee.FirstName, dto.FirstName);
        Assert.Equal(employee.LastName, dto.LastName);
        Assert.Equal(employee.Position, dto.Position);
        Assert.Equal(employee.Review, dto.Review);
    }

    [Fact]
    public void ToDto_MapsBookableServiceFields()
    {
        var employeeId = EmployeeId.New();
        var service = BusinessServiceEntity.Create(
            employeeId, "Haircut", "Classic haircut", 50m, TimeSpan.FromMinutes(45));

        var dto = service.ToDto();

        Assert.Equal(service.EmployeeId.Value, dto.EmployeeId);
        Assert.Equal(service.Id.Value, dto.BookableServiceId);
        Assert.Equal(service.ServiceName, dto.ServiceName);
        Assert.Equal(service.Description, dto.ServiceDescription);
        Assert.Equal(service.Price, dto.ServicePrice);
        Assert.Equal(service.Duration, dto.ServiceDuration);
    }
}
