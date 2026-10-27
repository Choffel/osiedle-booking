using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Domain.Services;

namespace Bookings.Domain.Tests.Services;

public class SlotCalculatorTests
{
    private static readonly DateTime DayStart = new(2026, 10, 15, 0, 0, 0);
    private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
    private static readonly TimeSpan TenMinutes = TimeSpan.FromMinutes(10);

    private static TimeSlot At(int hour, int minute, TimeSpan duration) =>
        TimeSlot.Create(DayStart.AddHours(hour).AddMinutes(minute), DayStart.AddHours(hour).AddMinutes(minute) + duration);

    [Fact]
    public async Task CalculateAvailableSlots_NoOccupiedSlots_FillsWorkingHours()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));

        var slots = await calculator.CalculateAvailableSlots(workingHours, [], OneHour, OneHour);

        Assert.NotEmpty(slots);
        Assert.All(slots, s => Assert.True(s.Start >= workingHours.Start && s.End <= workingHours.End));
    }

    [Fact]
    public async Task CalculateAvailableSlots_OneOccupiedSlot_SkipsBusyTime()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));
        var busy = At(10, 0, OneHour);

        var slots = await calculator.CalculateAvailableSlots(workingHours, [busy], OneHour, OneHour);

        Assert.DoesNotContain(slots, s => s.Start == busy.Start);
        Assert.DoesNotContain(slots, s => s.Overlaps(busy));
    }

    [Fact]
    public async Task CalculateAvailableSlots_FullyBookedDay_ReturnsEmpty()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));
        var busy = At(9, 0, TimeSpan.FromHours(9));

        var slots = await calculator.CalculateAvailableSlots(workingHours, [busy], OneHour, OneHour);

        Assert.Empty(slots);
    }

    [Fact]
    public async Task CalculateAvailableSlots_NonPositiveDuration_Throws()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => calculator.CalculateAvailableSlots(workingHours, [], TimeSpan.Zero, OneHour));
    }

    [Fact]
    public async Task CalculateAvailableSlots_NonPositiveStep_Throws()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => calculator.CalculateAvailableSlots(workingHours, [], OneHour, TimeSpan.Zero));
    }

    [Fact]
    public async Task RelevantSlot_InsideWorkingHours_ReturnsTrue()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));
        var slot = At(10, 0, OneHour);

        Assert.True(await calculator.RelevantSlot(slot, workingHours, CancellationToken.None));
    }

    [Fact]
    public async Task RelevantSlot_StartsBeforeWorkingHours_ReturnsFalse()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));
        var slot = TimeSlot.Create(DayStart.AddHours(8), DayStart.AddHours(9));

        Assert.False(await calculator.RelevantSlot(slot, workingHours, CancellationToken.None));
    }

    [Fact]
    public async Task RelevantSlot_EndsAfterWorkingHours_ReturnsFalse()
    {
        var calculator = new SlotCalculator();
        var workingHours = At(9, 0, TimeSpan.FromHours(9));
        var slot = At(17, 30, OneHour);

        Assert.False(await calculator.RelevantSlot(slot, workingHours, CancellationToken.None));
    }
}
