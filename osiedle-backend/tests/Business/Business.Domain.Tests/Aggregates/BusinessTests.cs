using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Domain.Tests.Aggregates;

public class BusinessTests
{
    private static readonly UserId OwnerId = UserId.New();
    private static readonly Address Location = Address.Create("Main Street", "1", "Warsaw");

    [Fact]
    public void Create_WithValidData_InitializesActiveAggregate()
    {
        var before = DateTime.UtcNow;

        var business = BusinessAggregate.Create("Beauty Salon", OwnerId, Location, "Haircuts and more");

        Assert.NotEqual(Guid.Empty, business.Id.Value);
        Assert.Equal("Beauty Salon", business.Name);
        Assert.Equal(OwnerId, business.OwnerId);
        Assert.Equal("Haircuts and more", business.Description);
        Assert.Equal(Location, business.Location);
        Assert.Equal(BusinessStatus.Active, business.Status);
        Assert.InRange(business.CreatedAt, before, DateTime.UtcNow);
        Assert.Null(business.UpdatedAt);
        Assert.Null(business.DeletedAt);
        Assert.Empty(business.Services);
        Assert.Empty(business.EmployeeIds);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create(name, OwnerId, Location, "Description"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_StoresNull(string? description)
    {
        var business = BusinessAggregate.Create("Name", OwnerId, Location, description);

        Assert.Null(business.Description);
    }

    [Fact]
    public void Create_WithEmptyLocation_Throws()
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", OwnerId, default, "Description"));
    }

    [Fact]
    public void Create_WithEmptyOwnerId_Throws()
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", new UserId(Guid.Empty), Location, "Description"));
    }

    [Fact]
    public void UpdateName_UpdatesFieldAndSetsUpdatedAt()
    {
        var business = BusinessAggregate.Create("Old name", OwnerId, Location);

        business.UpdateName("New name");

        Assert.Equal("New name", business.Name);
        Assert.NotNull(business.UpdatedAt);
    }

    [Fact]
    public void UpdateDescription_UpdatesFieldAndSetsUpdatedAt()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, Location, "Old description");

        business.UpdateDescription("New description");

        Assert.Equal("New description", business.Description);
        Assert.NotNull(business.UpdatedAt);
    }

    [Fact]
    public void UpdateAddress_UpdatesFieldAndSetsUpdatedAt()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, Location);
        var newLocation = Address.Create("New Street", "2", "Krakow");

        business.UpdateAddress(newLocation);

        Assert.Equal(newLocation, business.Location);
        Assert.NotNull(business.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithEmptyValue_Throws(string name)
    {
        var business = BusinessAggregate.Create("Old name", OwnerId, Location);

        Assert.Throws<ArgumentException>(() => business.UpdateName(name));
    }

    [Fact]
    public void UpdateAddress_WithEmptyValue_Throws()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, Location);

        Assert.Throws<ArgumentException>(() => business.UpdateAddress(default));
    }

    [Fact]
    public void ArchiveBusiness_SetsArchivedStatusAndDeletedAt()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, Location, "Description");

        business.ArchiveBusiness();

        Assert.Equal(BusinessStatus.Archived, business.Status);
        Assert.NotNull(business.DeletedAt);
    }

    [Fact]
    public void AddEmployee_AddsEmployeeOnlyOnce()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, Location, "Description");
        var employeeId = EmployeeId.New();

        business.AddEmployee(employeeId);
        business.AddEmployee(employeeId);

        Assert.Single(business.EmployeeIds);
        Assert.Contains(employeeId, business.EmployeeIds);
    }

    [Fact]
    public void AddService_WhenBusinessIsArchived_Throws()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, Location, "Description");
        business.ArchiveBusiness();

        Assert.Throws<InvalidOperationException>(() =>
            business.AddService(EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(30)));
    }
}
