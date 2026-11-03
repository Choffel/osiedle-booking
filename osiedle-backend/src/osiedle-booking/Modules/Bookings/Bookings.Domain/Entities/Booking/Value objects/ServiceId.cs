namespace Bookings.Domain.Entities.Booking.Value_objects;

public record struct ServiceId
{
    public Guid Value { get; init; }

    private ServiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceId cannot be empty.", nameof(value));

        Value = value;
    }
    
    public static ServiceId New() => new(Guid.NewGuid());
    
    public static ServiceId From(Guid value) => new(value);
    
    public static implicit operator Guid(ServiceId id) => id.Value;
    public override string ToString() => Value.ToString();
}