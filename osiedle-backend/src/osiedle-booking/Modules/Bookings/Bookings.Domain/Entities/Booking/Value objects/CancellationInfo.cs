using Bookings.Domain.Entities.Booking.Enum;

namespace Bookings.Domain.Entities.Booking.Value_objects;

public record struct CancellationInfo
{
    public CancelledBy By { get; init; }
    public Guid CancelledById { get; init; }
    public string Reason { get; init; }
    public DateTime CancelledAt { get; init; }
    

    private CancellationInfo(CancelledBy by, Guid cancelledById, string reason, DateTime cancelledAt)
    {
        if(by != CancelledBy.System && cancelledById == Guid.Empty)
            throw new ArgumentException("Cancelled by ID cannot be empty or null.", nameof(cancelledById));
        if(string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be null or empty.", nameof(reason));
        if (cancelledAt == default)
            throw new ArgumentException("Cancelled at cannot be the default value.", nameof(cancelledAt));
        
        By = by;
        CancelledById = cancelledById;
        Reason = reason;
        CancelledAt = cancelledAt;
    }

    public static CancellationInfo ByClient(ClientId clientId, DateTime cancelledAt)
    {
        // The domain requires a non-empty reason for every cancellation kind.
        return new CancellationInfo(CancelledBy.Client, clientId.Value, "Cancelled by client", cancelledAt);
    }
    
    public static CancellationInfo ByStaff(StaffId staffId, string reason, DateTime cancelledAt)
    {
        return new CancellationInfo(CancelledBy.Staff, staffId.Value, reason, cancelledAt);
    }
    
    public static CancellationInfo BySystem(string reason, DateTime cancelledAt)
    {
        return new CancellationInfo(CancelledBy.System, Guid.Empty, reason, cancelledAt);
    }
}