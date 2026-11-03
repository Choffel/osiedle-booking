namespace osiedle_booking.BuildingBlocks.ValueObjects;

public sealed record UserId // TODO сделать readonly record struct
{
    public Guid Value { get; init; }
    
    private UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value)
    {
        return new(value);
    }
    
    public static implicit operator Guid(UserId id) => id.Value;
    public override string ToString() => Value.ToString();
}