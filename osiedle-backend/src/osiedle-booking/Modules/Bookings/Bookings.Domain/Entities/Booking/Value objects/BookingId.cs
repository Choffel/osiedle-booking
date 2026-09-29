namespace Bookings.Domain.Entities.Booking.Value_objects;

public sealed record BookingId(Guid Value)
{
    public static BookingId New() => new(Guid.NewGuid());
}