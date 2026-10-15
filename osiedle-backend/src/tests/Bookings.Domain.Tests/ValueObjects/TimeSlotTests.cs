using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Domain.Exceptions;

namespace Bookings.Domain.Tests.ValueObjects;

public class TimeSlotTests
{
    [Fact]
    public void Create_ValidRange_ReturnsSlotWithCorrectValues()
    {
        var start = new DateTime(2026, 10, 15, 9, 0, 0);
        var end = new DateTime(2026, 10, 15, 10, 0, 0);

        var slot = TimeSlot.Create(start, end);

        Assert.Equal(start, slot.Start);
        Assert.Equal(end, slot.End);
    }

    [Fact]
    public void Create_EndEqualsStart_Throws()
    {
        var start = new DateTime(2026, 10, 15, 9, 0, 0);

        Assert.Throws<InvalidTimeSlotRangeException>(() => TimeSlot.Create(start, start));
    }

    [Fact]
    public void Create_EndBeforeStart_Throws()
    {
        var start = new DateTime(2026, 10, 15, 10, 0, 0);
        var end = new DateTime(2026, 10, 15, 9, 0, 0);

        Assert.Throws<InvalidTimeSlotRangeException>(() => TimeSlot.Create(start, end));
    }

    [Fact]
    public void Overlaps_PartiallyOverlappingSlots_ReturnsTrue()
    {
        var slot = TimeSlot.Create(new DateTime(2026, 10, 15, 9, 0, 0), new DateTime(2026, 10, 15, 10, 0, 0));
        var other = TimeSlot.Create(new DateTime(2026, 10, 15, 9, 30, 0), new DateTime(2026, 10, 15, 10, 30, 0));

        Assert.True(slot.Overlaps(other));
    }

    [Fact]
    public void Overlaps_DisjointSlots_ReturnsFalse()
    {
        var slot = TimeSlot.Create(new DateTime(2026, 10, 15, 9, 0, 0), new DateTime(2026, 10, 15, 10, 0, 0));
        var other = TimeSlot.Create(new DateTime(2026, 10, 15, 10, 0, 0), new DateTime(2026, 10, 15, 11, 0, 0));

        Assert.False(slot.Overlaps(other));
    }

    [Fact]
    public void Overlaps_SameStart_ReturnsTrue()
    {
        var slot = TimeSlot.Create(new DateTime(2026, 10, 15, 9, 0, 0), new DateTime(2026, 10, 15, 10, 0, 0));
        var other = TimeSlot.Create(new DateTime(2026, 10, 15, 9, 0, 0), new DateTime(2026, 10, 15, 9, 30, 0));

        Assert.True(slot.Overlaps(other));
    }

    [Fact]
    public void Overlaps_DifferentDays_ReturnsFalse()
    {
        var slot = TimeSlot.Create(new DateTime(2026, 10, 15, 9, 0, 0), new DateTime(2026, 10, 15, 10, 0, 0));
        var other = TimeSlot.Create(new DateTime(2026, 10, 16, 9, 0, 0), new DateTime(2026, 10, 16, 10, 0, 0));

        Assert.False(slot.Overlaps(other));
    }

    [Fact]
    public void Records_WithSameValues_AreEqual()
    {
        var start = new DateTime(2026, 10, 15, 9, 0, 0);
        var end = new DateTime(2026, 10, 15, 10, 0, 0);

        var slot1 = TimeSlot.Create(start, end);
        var slot2 = TimeSlot.Create(start, end);

        Assert.Equal(slot1, slot2);
    }
}
