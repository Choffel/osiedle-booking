namespace Bookings.Domain.Entities.Booking.Value_objects;

public record struct ClientId
{
    public Guid Value { get; init; }
    

    private ClientId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(value));

        Value = value;
    }
    
    public static ClientId New() => new(Guid.NewGuid());
    
    public static ClientId From(Guid value) => new(value);
    
    public static implicit operator Guid(ClientId id) => id.Value;
    public override string ToString() => Value.ToString();
}

