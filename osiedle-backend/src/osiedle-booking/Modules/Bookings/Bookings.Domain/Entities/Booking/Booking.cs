using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Entities.Booking.Value_objects;
using osiedle_booking.BuildingBlocks.Domain;

namespace Bookings.Domain.Entities.Booking;

public class Booking : AggregateRoot<BookingId>
{
    public StaffId StaffId { get; private set; }
    public BusinessId BusinessId { get; private set; } 
    public ClientId ClientId { get; private set; }
    public ServiceId ServiceId { get; private set; }
    public TimeSlot Slot { get; private set; }
    public Money Price { get; private set; } 
    public BookingStatus Status { get; private set; }
    public CancellationInfo? Cancellation { get; private set; }
    
    private Booking() { } // EF

    private  Booking(BookingId id, StaffId staffId, BusinessId businessId, ClientId clientId, ServiceId serviceId,
        TimeSlot slot, Money price, BookingStatus status): base(id)   
    {
        StaffId = staffId;
        BusinessId = businessId;
        ClientId = clientId;
        ServiceId = serviceId;
        Slot = slot;
        Price = price;
        Status = status;
    }

    public static Booking Create(
        StaffId staffId,
        BusinessId businessId,
        ClientId clientId,
        ServiceId serviceId,
        TimeSlot slot,
        Money price
        )
    {
        var booking = new Booking
        (
            BookingId.New(),
            staffId,
            businessId,
            clientId,
            serviceId,
            slot,
            price,
            BookingStatus.Confirmed
        );
        
        return booking;
    }

    public void CancelByClient(ClientId clientId, DateTime cancelledAt)
    {
        EnsureCanBeCancelled();
        
        if(ClientId != clientId)
            throw new ArgumentException("Only the booking owner can cancel the booking", nameof(clientId));
        
        
        Status = BookingStatus.Cancelled;
        Cancellation = CancellationInfo.ByClient(clientId, cancelledAt);
    }
    
    public void CancelByStaff(StaffId staffId, string reason, DateTime cancelledAt)
    {
       EnsureCanBeCancelled();

        Status = BookingStatus.Cancelled;
        Cancellation = CancellationInfo.ByStaff(staffId, reason, cancelledAt);
    }
    
    public void CancelBySystem(string reason, DateTime cancelledAt)
    {
       EnsureCanBeCancelled();

        Status = BookingStatus.Cancelled;
        Cancellation = CancellationInfo.BySystem(reason, cancelledAt);
    }

    public void Complete()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException($"Cannot complete a cancelled booking {Id}.");
        
        if (Status == BookingStatus.Completed)
            return;
        
        Status = BookingStatus.Completed;
    }
    
    public bool IsOwnedBy(ClientId clientId)
    {
        return ClientId == clientId;
    }
    
    private void EnsureCanBeCancelled()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException($"Booking {Id} is already cancelled.");

        if (Status == BookingStatus.Completed)
            throw new InvalidOperationException($"Cannot cancel booking {Id} because it is already completed.");
    }
}