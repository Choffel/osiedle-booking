using osiedle_booking.BuildingBlocks;

namespace Bookings.Domain.Errors;

public static class BookingError
{
    public static readonly Error Overlapping =
        Error.Failure("Booking.Overlapping", "Booking overlaps with existing booking");
    
    public static readonly Error NotFound =
        Error.NotFound("Booking.NotFound", "Booking was not found");
    
    public static readonly Error NotOwnedByClient =
        Error.Failure("Booking.NotOwnedByClient", "You cannot delete a reservation that isn't yours.");
}