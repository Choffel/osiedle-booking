namespace Business.Domain.ValueObject;

public readonly record struct BusinessId
{
    public Guid Value { get; }

    public BusinessId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BusinessId cannot be empty.", nameof(value));
        
        Value = value;
    }

    public static BusinessId New() => new BusinessId(Guid.NewGuid());
    
    public static BusinessId From(Guid value) => new(value);
    
    public static implicit operator Guid(BusinessId businessId) => businessId.Value;
    
    public override string ToString() => Value.ToString();
}