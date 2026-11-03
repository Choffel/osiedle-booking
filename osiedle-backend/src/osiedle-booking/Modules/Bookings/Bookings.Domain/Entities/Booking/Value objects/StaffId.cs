namespace Bookings.Domain.Entities.Booking.Value_objects;

public record struct StaffId
{
    public Guid Value { get; init; }

    private StaffId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StaffId cannot be empty.", nameof(value));

        Value = value;
    }
    
    public static StaffId New() => new(Guid.NewGuid());
    
    public static StaffId From(Guid value) => new(value);
    
    public static implicit operator Guid(StaffId id) => id.Value;
    public override string ToString() => Value.ToString();
}