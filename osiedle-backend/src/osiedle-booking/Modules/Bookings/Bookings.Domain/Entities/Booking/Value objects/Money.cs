namespace Bookings.Domain.Entities.Booking.Value_objects;

public sealed record Money(decimal Amount, string Currency = "PLN");