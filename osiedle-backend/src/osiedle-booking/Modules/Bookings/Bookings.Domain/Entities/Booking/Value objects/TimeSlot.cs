namespace Bookings.Domain.Entities.Booking.Value_objects;

public sealed record TimeSlot
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    
    private TimeSlot() {}

    private TimeSlot(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time.", nameof(end));
        
        Start = start;
        End = end;
    }

    public static TimeSlot Create(DateTime start, DateTime end)
    {
        return new TimeSlot(start, end);
    }

    public bool Overlaps(TimeSlot other) =>
        Start < other.End && other.Start < End;
}