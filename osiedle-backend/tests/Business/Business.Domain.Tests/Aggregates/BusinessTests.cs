using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using BusinessAggregate = Business.Domain.Aggregates.Business;

namespace Business.Domain.Tests.Aggregates;

public class BusinessTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_InitializesActiveAggregate()
    {
        var before = DateTime.UtcNow;

        var business = BusinessAggregate.Create("Beauty Salon", OwnerId, "Haircuts and more", "Main Street 1");

        Assert.NotEqual(Guid.Empty, business.Id.Value);
        Assert.Equal("Beauty Salon", business.Name);
        Assert.Equal(OwnerId, business.OwnerId);
        Assert.Equal("Haircuts and more", business.Description);
        Assert.Equal("Main Street 1", business.Location);
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
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create(name, OwnerId, "Description", "Location"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_Throws(string description)
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", OwnerId, description, "Location"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyLocation_Throws(string location)
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", OwnerId, "Description", location));
    }

    [Fact]
    public void Create_WithEmptyOwnerId_Throws()
    {
        Assert.Throws<ArgumentException>(() => BusinessAggregate.Create("Name", Guid.Empty, "Description", "Location"));
    }

    [Fact]
    public void UpdateInfo_UpdatesFieldsAndSetsUpdatedAt()
    {
        var business = BusinessAggregate.Create("Old name", OwnerId, "Old description", "Old location");

        business.UpdateInfo("New name", "New description", "New location");

        Assert.Equal("New name", business.Name);
        Assert.Equal("New description", business.Description);
        Assert.Equal("New location", business.Location);
        Assert.NotNull(business.UpdatedAt);
    }

    [Theory]
    [InlineData("", "Description", "Location")]
    [InlineData("Name", "", "Location")]
    [InlineData("Name", "Description", "")]
    public void UpdateInfo_WithEmptyValues_Throws(string name, string description, string location)
    {
        var business = BusinessAggregate.Create("Old name", OwnerId, "Old description", "Old location");

        Assert.Throws<ArgumentException>(() => business.UpdateInfo(name, description, location));
    }

    [Fact]
    public void ArchiveBusiness_SetsArchivedStatusAndDeletedAt()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, "Description", "Location");

        business.ArchiveBusiness();

        Assert.Equal(BusinessStatus.Archived, business.Status);
        Assert.NotNull(business.DeletedAt);
    }

    [Fact]
    public void AddEmployee_AddsEmployeeOnlyOnce()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, "Description", "Location");
        var employeeId = EmployeeId.New();

        business.AddEmployee(employeeId);
        business.AddEmployee(employeeId);

        Assert.Single(business.EmployeeIds);
        Assert.Contains(employeeId, business.EmployeeIds);
    }

    [Fact]
    public void AddService_WhenBusinessIsArchived_Throws()
    {
        var business = BusinessAggregate.Create("Name", OwnerId, "Description", "Location");
        business.ArchiveBusiness();

        Assert.Throws<InvalidOperationException>(() =>
            business.AddService(EmployeeId.New(), "Haircut", "Description", 50m, TimeSpan.FromMinutes(30)));
    }
}
