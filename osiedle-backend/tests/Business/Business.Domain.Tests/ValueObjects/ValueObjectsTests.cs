using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;

namespace Business.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Fact]
    public void New_CreatesUniqueIdentifiers()
    {
        Assert.NotEqual(BusinessId.New(), BusinessId.New());
        Assert.NotEqual(EmployeeId.New(), EmployeeId.New());
        Assert.NotEqual(BookableServiceId.New(), BookableServiceId.New());
        Assert.NotEqual(WorkingHoursId.New(), WorkingHoursId.New());
    }

    [Fact]
    public void From_WithEmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => BusinessId.From(Guid.Empty));
        Assert.Throws<ArgumentException>(() => EmployeeId.From(Guid.Empty));
        Assert.Throws<ArgumentException>(() => BookableServiceId.From(Guid.Empty));
        Assert.Throws<ArgumentException>(() => WorkingHoursId.From(Guid.Empty));
    }

    [Fact]
    public void From_KeepsProvidedValue()
    {
        var value = Guid.NewGuid();

        Assert.Equal(value, BusinessId.From(value).Value);
        Assert.Equal(value, EmployeeId.From(value).Value);
        Assert.Equal(value, BookableServiceId.From(value).Value);
        Assert.Equal(value, WorkingHoursId.From(value).Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FromNullable_ReturnsNullForEmptyValue(bool nullValue)
    {
        Guid? value = nullValue ? null : Guid.Empty;

        Assert.Null(BusinessId.FromNullable(value));
        Assert.Null(EmployeeId.FromNullable(value));
    }

    [Fact]
    public void FromNullable_ReturnsIdentifierForNonEmptyValue()
    {
        var value = Guid.NewGuid();

        Assert.Equal(value, BusinessId.FromNullable(value)!.Value.Value);
        Assert.Equal(value, EmployeeId.FromNullable(value)!.Value.Value);
    }

    [Fact]
    public void ImplicitConversionToGuid_ReturnsUnderlyingValue()
    {
        var value = Guid.NewGuid();

        Guid businessId = BusinessId.From(value);
        Guid employeeId = EmployeeId.From(value);
        Guid bookableServiceId = BookableServiceId.From(value);
        Guid workingHoursId = WorkingHoursId.From(value);

        Assert.Equal(value, businessId);
        Assert.Equal(value, employeeId);
        Assert.Equal(value, bookableServiceId);
        Assert.Equal(value, workingHoursId);
    }
}
