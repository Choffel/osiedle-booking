namespace Bookings.Domain.Entities.Booking.Value_objects;

public sealed record BookingId
{
    public Guid Value { get; init; }
    
    private BookingId() { }

    private BookingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BookingId cannot be empty.", nameof(value));

        Value = value;
    }
    
    public static BookingId New() => new(Guid.NewGuid());
    
    public static BookingId From(Guid value) => new(value);
    
    public static implicit operator Guid(BookingId id) => id.Value;
    public override string ToString() => Value.ToString();
}