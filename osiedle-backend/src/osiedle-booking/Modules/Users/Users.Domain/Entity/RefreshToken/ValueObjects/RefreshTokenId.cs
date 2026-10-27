namespace Users.Domain.Entity.RefreshToken.ValueObjects;

public readonly record struct RefreshTokenId
{
    public Guid Value { get; }
    
    public RefreshTokenId()
    {
        throw new InvalidOperationException("Default constructor is not allowed for RefreshTokenId.");
    }
    
    private RefreshTokenId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RefreshTokenId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static RefreshTokenId New() => new(Guid.NewGuid());
    public static RefreshTokenId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(value));
            
        return new(value);
    }
    
    public static implicit operator Guid(RefreshTokenId id) => id.Value;
    public override string ToString() => Value.ToString();
}