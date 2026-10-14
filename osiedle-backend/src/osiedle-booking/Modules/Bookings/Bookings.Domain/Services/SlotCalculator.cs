using Bookings.Domain.Entities.Booking.Value_objects;
    
namespace Bookings.Domain.Services;

public sealed class SlotCalculator : ISlotCalculator
{
    public async Task<IReadOnlyList<TimeSlot>> CalculateAvailableSlots(
        TimeSlot workingHours,
        IReadOnlyList<TimeSlot> occupiedSlots,
        TimeSpan serviceDuration,
        TimeSpan step)
    {
        if (serviceDuration <= TimeSpan.Zero || step <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(serviceDuration));

        var result = new List<TimeSlot>();
        var cursor = workingHours.Start;

        var relevant = occupiedSlots
            .Where(b => b.End > workingHours.Start && b.Start < workingHours.End)
            .OrderBy(b => b.Start);

        foreach (var busy in relevant)
        {
            AddFreeSlots(cursor, busy.Start);
            if (busy.End > cursor)
                cursor = busy.End;
        }

        AddFreeSlots(cursor, workingHours.End);
        return result;

        void AddFreeSlots(DateTime from, DateTime to)
        {
            for (var start = from; start + serviceDuration <= to; start += step)
                result.Add(TimeSlot.Create(start, start + serviceDuration));
        }
    }
}