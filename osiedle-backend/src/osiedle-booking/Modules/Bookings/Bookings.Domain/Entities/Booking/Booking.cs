using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Domain.Exceptions;
using osiedle_booking.BuildingBlocks.Domain;

namespace Bookings.Domain.Entities.Booking;

public class Booking : AggregateRoot<BookingId>
{
    public StaffId StaffId { get; private set; }
    public BusinessId BusinessId { get; private set; } 
    public ClientId ClientId { get; private set; }
    public ServiceId ServiceId { get; private set; }
    public TimeSlot Slot { get; private set; }
    public BookingStatus Status { get; private set; }
    public Money Price { get; private set; } 
    public CancellationInfo? Cancellation { get; private set; }
    
    private Booking() { } // EF
    
    public static Booking Create(
        BusinessId businessId,
        StaffId staffId,
        ClientId clientId,
        ServiceId serviceId,
        TimeSlot slot,
        Money price
        )
    {
        

        var booking = new Booking
        {
            Id = BookingId.New(),
            BusinessId = businessId,
            StaffId = staffId,
            ClientId = clientId,
            ServiceId = serviceId,
            Slot = slot,
            Price = price,
            Status = BookingStatus.Confirmed,
            CreatedAt = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)
        };

        /*booking.RaiseDomainEvent(new BookingCreatedDomainEvent(
            booking.Id, businessId, staffId, clientId, slot));*/

        return booking;
    }
}