namespace Business.Domain.ValueObjects;

public readonly record struct TimeSlot
{
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    private TimeSlot(TimeOnly startTime, TimeOnly endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public static TimeSlot Create(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));
        if (endTime - startTime < TimeSpan.FromMinutes(15))
            throw new ArgumentException("Time slot duration must be at least 15 minutes.");

        return new TimeSlot(startTime, endTime);
    }

    public TimeSpan Duration => EndTime - StartTime;
    
    public bool OverlapsWith(TimeSlot other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public bool IsEmpty => StartTime == default && EndTime == default;

    public override string ToString() => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
}