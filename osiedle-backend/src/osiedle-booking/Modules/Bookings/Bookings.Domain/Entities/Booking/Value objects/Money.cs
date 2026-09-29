namespace Bookings.Domain.Entities.Value_objects;

public sealed record Money(decimal Amount, string Currency = "PLN");