namespace Business.Domain.ValueObject;

public readonly record struct BookableServiceId
{
    public Guid Value { get; }

    private  BookableServiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BookableServiceId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static BookableServiceId New() => new BookableServiceId(Guid.NewGuid());
    
    public static BookableServiceId From(Guid value) => new BookableServiceId(value);
    
    public static implicit operator Guid(BookableServiceId id) => id.Value;
    
    public override string ToString() => Value.ToString();
    
}
