namespace Bookings.Domain.Entities.Value_objects;

public sealed record CancellationInfo(CancelledBy By, string? Reason, DateTime CancelledAt);