namespace Users.Domain.Entity.RefreshToken.ValueObjects;

public record struct RefreshTokenId
{
    public Guid Value { get; init; }

    private RefreshTokenId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RefreshTokenId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static RefreshTokenId New() => new(Guid.NewGuid());
    public static RefreshTokenId From(Guid value)
    {
        return new(value);
    }
    
    public static implicit operator Guid(RefreshTokenId id) => id.Value;
    public override string ToString() => Value.ToString();
}