using osiedle_booking.BuildingBlocks.Domain;

namespace Bookings.Domain.Exceptions;

public class InvalidBookingPriceException : DomainException
{
    public InvalidBookingPriceException(decimal attemptedAmount)
        : base($"Booking price cannot be negative: {attemptedAmount}.")
    {
        AttemptedAmount = attemptedAmount;
    }

    public decimal AttemptedAmount { get; }
}
