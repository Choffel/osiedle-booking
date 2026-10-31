namespace osiedle_booking.BuildingBlocks.ValueObjects;

public sealed record UserId
{
    public Guid Value { get; }
    
    public UserId()
    {
        throw new InvalidOperationException("Default constructor is not allowed for UserId.");
    }
    
    private UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(value));
            
        return new(value);
    }
    
    public static implicit operator Guid(UserId id) => id.Value;
    public override string ToString() => Value.ToString();
}