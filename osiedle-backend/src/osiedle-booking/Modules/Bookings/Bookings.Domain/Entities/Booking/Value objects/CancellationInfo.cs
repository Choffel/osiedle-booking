using Bookings.Domain.Entities.Booking.Enum;

namespace Bookings.Domain.Entities.Booking.Value_objects;

public sealed record CancellationInfo(CancelledBy By, string? Reason, DateTime CancelledAt);