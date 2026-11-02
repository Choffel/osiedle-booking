namespace Bookings.Domain.Entities.Booking.Value_objects;

public sealed record BusinessId
{
    public Guid Value { get; init; }
    
    private BusinessId() { }

    private BusinessId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BusinessId cannot be empty.", nameof(value));

        Value = value;
    }
    
    public static BusinessId New() => new(Guid.NewGuid());
    
    public static BusinessId From(Guid value) => new(value);
    
    public static implicit operator Guid(BusinessId id) => id.Value;
    public override string ToString() => Value.ToString();
}