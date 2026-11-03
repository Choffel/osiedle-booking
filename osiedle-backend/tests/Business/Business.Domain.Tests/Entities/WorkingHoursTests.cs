using Business.Domain.Entities;
using Business.Domain.ValueObjects;

namespace Business.Domain.Tests.Entities;

public class WorkingHoursTests
{
    private static readonly DateOnly Date = new(2026, 10, 23);

    [Fact]
    public void Create_WithValidRange_InitializesEntity()
    {
        var employeeId = EmployeeId.New();
        var before = DateTime.UtcNow;

        var workingHours = WorkingHours.Create(employeeId, Date, new TimeOnly(9, 0), new TimeOnly(17, 0));

        Assert.NotEqual(Guid.Empty, workingHours.Id.Value);
        Assert.Equal(employeeId, workingHours.EmployeeId);
        Assert.Equal(Date, workingHours.Date);
        Assert.Equal(new TimeOnly(9, 0), workingHours.StartTime);
        Assert.Equal(new TimeOnly(17, 0), workingHours.EndTime);
        Assert.InRange(workingHours.CreatedAt, before, DateTime.UtcNow);
        Assert.Null(workingHours.UpdatedAt);
    }

    [Theory]
    [InlineData(9, 0, 9, 0)]
    [InlineData(17, 0, 9, 0)]
    public void Create_WithInvalidRange_Throws(int startHour, int startMinute, int endHour, int endMinute)
    {
        Assert.Throws<ArgumentException>(() => WorkingHours.Create(
            EmployeeId.New(),
            Date,
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute)));
    }

    [Fact]
    public void UpdateTime_WithValidRange_UpdatesFieldsAndSetsUpdatedAt()
    {
        var workingHours = WorkingHours.Create(EmployeeId.New(), Date, new TimeOnly(9, 0), new TimeOnly(17, 0));

        workingHours.UpdateTime(new TimeOnly(10, 30), new TimeOnly(18, 30));

        Assert.Equal(new TimeOnly(10, 30), workingHours.StartTime);
        Assert.Equal(new TimeOnly(18, 30), workingHours.EndTime);
        Assert.NotNull(workingHours.UpdatedAt);
    }

    [Theory]
    [InlineData(18, 0, 10, 0)]
    [InlineData(10, 0, 10, 0)]
    public void UpdateTime_WithInvalidRange_Throws(int startHour, int startMinute, int endHour, int endMinute)
    {
        var workingHours = WorkingHours.Create(EmployeeId.New(), Date, new TimeOnly(9, 0), new TimeOnly(17, 0));

        Assert.Throws<ArgumentException>(() =>
            workingHours.UpdateTime(new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute)));
    }
}
