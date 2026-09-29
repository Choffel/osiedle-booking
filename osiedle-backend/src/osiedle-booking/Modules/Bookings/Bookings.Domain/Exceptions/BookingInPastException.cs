using osiedle_booking.BuildingBlocks.Domain;

namespace Bookings.Domain.Exceptions;

public class BookingInPastException : DomainException
{
    public BookingInPastException(DateTime attemptedStart)
        : base($"Cannot reserve a date in the past: {attemptedStart:O}")
    {
        AttemptedStart = attemptedStart;
    }

    public DateTime AttemptedStart { get; }
}