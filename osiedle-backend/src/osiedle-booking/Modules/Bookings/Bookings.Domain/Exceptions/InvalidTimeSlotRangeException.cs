using osiedle_booking.BuildingBlocks.Domain;

namespace Bookings.Domain.Exceptions;

public class InvalidTimeSlotRangeException : DomainException
{
    public InvalidTimeSlotRangeException(DateTime start, DateTime end)
        : base($"End time ({end}) must be after start time ({start}).")
    {
    }
}