using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Domain.Tests.Aggregates;

public class BusinessTests
{
    private static readonly UserId OwnerId = UserId.New();
    private static readonly Location ValidLocation = Location.Create("Warsaw", "Main Street 1");

    [Fact]
    public void Create_WithValidData_InitializesActiveAggregate()
    {
        var before = DateTime.UtcNow;

        var business = BusinessAggregate.Create("Beauty Salon", OwnerId, "Haircuts and more", ValidLocation, Category.Haircut);

        Assert.NotEqual(Guid.Empty, business.Id.Value);
        Assert.Equal("Beauty Salon", business.Name);
        Assert.Equal(OwnerId, business.OwnerId);
        Assert.Equal("Haircuts and more", business.Description);
        Assert.Equal(ValidLocation, business.Location);
        Assert.Equal("Warsaw", business.Location.City);
        Assert.Equal("Main Street 1", business.Location.Street);
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
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create(name, OwnerId, "Description", ValidLocation, Category.Haircut));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_Throws(string description)
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", OwnerId, description, ValidLocation, Category.Haircut));
    }

    [Fact]
    public void Create_WithDefaultLocation_Throws()
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", OwnerId, "Description", default, Category.Haircut));
    }

    [Fact]
    public void Create_WithEmptyOwnerId_Throws()
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", UserId.From(Guid.Empty), "Description", ValidLocation, Category.Haircut));
    }

    [Fact]
    public void UpdateInfo_UpdatesFieldsAndSetsUpdatedAt()
    {
        var business = BusinessAggregate.Create("Old name", OwnerId, "Old description", ValidLocation, Category.Haircut);
        var newLocation = Location.Create("Krakow", "New Street 2");

        business.UpdateInfo("New name", "New description", newLocation);

        Assert.Equal("New name", business.Name);
        Assert.Equal("New description", business.Description);
        Assert.Equal(newLocation, business.Location);
        Assert.NotNull(business.UpdatedAt);
    }

    [Fact]
    public void UpdateInfo_WithDefaultLocation_Throws()
    {
        var business = BusinessAggregate.Create("Old name", OwnerId, "Old description", ValidLocation, Category.Haircut);

        Assert.Throws<ArgumentException>(() => business.UpdateInfo("New name", "New description", default));
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData("Name", "")]
    public void UpdateInfo_WithEmptyValues_Throws(string name, string description)
    {
        var business = BusinessAggregate.Create("Old name", OwnerId, "Old description", ValidLocation, Category.Haircut);

        Assert.Throws<ArgumentException>(() => business.UpdateInfo(name, description, ValidLocation));
    }

    [Fact]
    public void ArchiveBusiness_SetsArchivedStatusAndDeletedAt()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, "Description", ValidLocation, Category.Haircut);

        business.ArchiveBusiness();

        Assert.Equal(BusinessStatus.Archived, business.Status);
        Assert.NotNull(business.DeletedAt);
    }

    [Fact]
    public void AddEmployee_AddsEmployeeOnlyOnce()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, "Description", ValidLocation, Category.Haircut);
        var employeeId = EmployeeId.New();

        business.AddEmployee(employeeId);
        business.AddEmployee(employeeId);

        Assert.Single(business.EmployeeIds);
        Assert.Contains(employeeId, business.EmployeeIds);
    }

    [Fact]
    public void AddService_WhenBusinessIsArchived_Throws()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, "Description", ValidLocation, Category.Haircut);
        business.ArchiveBusiness();

        Assert.Throws<InvalidOperationException>(() =>
            business.AddService(business.Id, EmployeeId.New(), Category.Haircut, "Haircut", "Description", 50m, TimeSpan.FromMinutes(30)));
    }
}
