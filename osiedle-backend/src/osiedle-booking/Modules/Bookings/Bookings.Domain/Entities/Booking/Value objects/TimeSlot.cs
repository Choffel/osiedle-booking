using Bookings.Domain.Exceptions;

namespace Bookings.Domain.Entities.Booking.Value_objects;

public sealed record TimeSlot
{
    public DateTime Start { get; }
    public DateTime End { get; }

    private TimeSlot(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public static TimeSlot Create(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new InvalidTimeSlotRangeException(start, end);

        return new TimeSlot(start, end);
    }

    public bool Overlaps(TimeSlot other) =>
        Start < other.End && other.Start < End;
}