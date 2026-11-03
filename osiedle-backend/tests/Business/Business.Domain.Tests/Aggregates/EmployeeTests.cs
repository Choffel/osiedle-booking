using Business.Domain.Aggregates;
using Business.Domain.Enums;
using Business.Domain.ValueObjects;

namespace Business.Domain.Tests.Aggregates;

public class EmployeeTests
{
    [Fact]
    public void Create_WithValidData_InitializesActiveAggregate()
    {
        var businessId = BusinessId.New();
        var before = DateTime.UtcNow;

        var employee = Employee.Create(businessId, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");

        Assert.NotEqual(Guid.Empty, employee.Id.Value);
        Assert.Equal(businessId, employee.BusinessId);
        Assert.Equal("Jan", employee.FirstName);
        Assert.Equal("Kowalski", employee.LastName);
        Assert.Equal("Barber", employee.Position);
        Assert.Equal("Great", employee.Review);
        Assert.Equal(EmploymentStatus.Active, employee.Status);
        Assert.InRange(employee.CreatedAt, before, DateTime.UtcNow);
        Assert.Null(employee.UpdatedAt);
        Assert.Empty(employee.BookableServices);
    }

    [Fact]
    public void Create_NormalizesEmailToLowerCaseAndTrims()
    {
        var employee = Employee.Create(null, "Jan", "Kowalski", "Barber", "Great", "  Jan.Kowalski@Example.COM  ");

        Assert.Equal("jan.kowalski@example.com", employee.Email);
    }

    [Fact]
    public void Create_WithoutBusiness_LeavesBusinessIdNull()
    {
        var employee = Employee.Create(null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");

        Assert.Null(employee.BusinessId);
    }

    [Theory]
    [InlineData("", "Kowalski", "Barber", "Great", "jan@example.com")]
    [InlineData("Jan", "", "Barber", "Great", "jan@example.com")]
    [InlineData("Jan", "Kowalski", "", "Great", "jan@example.com")]
    [InlineData("Jan", "Kowalski", "Barber", "Great", "")]
    public void Create_WithInvalidData_Throws(
        string firstName,
        string lastName,
        string position,
        string review,
        string email)
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(null, firstName, lastName, position, review, email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutReview_LeavesReviewEmpty(string? review)
    {
        var employee = Employee.Create(null, "Jan", "Kowalski", "Barber", review, "jan@example.com");

        Assert.Null(employee.Review);
    }

    [Fact]
    public void UpdateInfo_UpdatesFieldsAndSetsUpdatedAt()
    {
        var employee = Employee.Create(null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");

        employee.UpdateInfo("Adam", "Nowak", "Stylist", "Very good", "Adam.Nowak@Example.COM");

        Assert.Equal("Adam", employee.FirstName);
        Assert.Equal("Nowak", employee.LastName);
        Assert.Equal("Stylist", employee.Position);
        Assert.Equal("Very good", employee.Review);
        Assert.Equal("adam.nowak@example.com", employee.Email);
        Assert.NotNull(employee.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_SetsNewStatus()
    {
        var employee = Employee.Create(null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");

        employee.ChangeStatus(EmploymentStatus.OnLeave);

        Assert.Equal(EmploymentStatus.OnLeave, employee.Status);
    }

    [Fact]
    public void ChangeStatus_ToTheSameStatus_DoesNotThrow()
    {
        var employee = Employee.Create(null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");

        employee.ChangeStatus(EmploymentStatus.Active);

        Assert.Equal(EmploymentStatus.Active, employee.Status);
    }

    [Fact]
    public void ChangeStatus_FromTerminated_Throws()
    {
        var employee = Employee.Create(null, "Jan", "Kowalski", "Barber", "Great", "jan@example.com");
        employee.ChangeStatus(EmploymentStatus.Terminated);

        Assert.Throws<InvalidOperationException>(() => employee.ChangeStatus(EmploymentStatus.Active));
    }
}
