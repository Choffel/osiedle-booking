using Business.Domain.Entities;
using Business.Domain.ValueObjects;

namespace Business.Domain.Tests.Entities;

public class WorkingHoursTests
{
    private static readonly DateOnly Date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

    [Fact]
    public void Create_WithValidRange_InitializesEntity()
    {
        var employeeId = EmployeeId.New();
        var before = DateTime.UtcNow;
        var slot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(17, 0));

        var workingHours = WorkingHours.Create(employeeId, Date, slot);

        Assert.NotEqual(Guid.Empty, workingHours.Id.Value);
        Assert.Equal(employeeId, workingHours.EmployeeId);
        Assert.Equal(Date, workingHours.Date);
        Assert.Equal(new TimeOnly(9, 0), workingHours.Slot.StartTime);
        Assert.Equal(new TimeOnly(17, 0), workingHours.Slot.EndTime);
        Assert.InRange(workingHours.CreatedAt, before, DateTime.UtcNow);
        Assert.Null(workingHours.UpdatedAt);
    }

    [Theory]
    [InlineData(9, 0, 9, 0)]
    [InlineData(17, 0, 9, 0)]
    public void Create_WithInvalidRange_Throws(int startHour, int startMinute, int endHour, int endMinute)
    {
        Assert.Throws<ArgumentException>(() => TimeSlot.Create(
            new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute)));
    }

    [Fact]
    public void Create_WithPastDate_Throws()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var slot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(17, 0));

        Assert.Throws<InvalidOperationException>(() =>
            WorkingHours.Create(EmployeeId.New(), pastDate, slot));
    }

    [Fact]
    public void UpdateSlot_WithValidRange_UpdatesFieldsAndSetsUpdatedAt()
    {
        var workingHours = WorkingHours.Create(
            EmployeeId.New(),
            Date,
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(17, 0)));
        var newSlot = TimeSlot.Create(new TimeOnly(10, 30), new TimeOnly(18, 30));

        workingHours.UpdateSlot(newSlot);

        Assert.Equal(new TimeOnly(10, 30), workingHours.Slot.StartTime);
        Assert.Equal(new TimeOnly(18, 30), workingHours.Slot.EndTime);
        Assert.NotNull(workingHours.UpdatedAt);
    }

    [Theory]
    [InlineData(18, 0, 10, 0)]
    [InlineData(10, 0, 10, 0)]
    public void UpdateSlot_WithInvalidRange_Throws(int startHour, int startMinute, int endHour, int endMinute)
    {
        var workingHours = WorkingHours.Create(
            EmployeeId.New(),
            Date,
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(17, 0)));

        Assert.Throws<ArgumentException>(() =>
            workingHours.UpdateSlot(
                TimeSlot.Create(new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute))));
    }
}
