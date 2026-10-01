namespace Business.Domain.ValueObject;

public readonly record struct BusinessServiceId
{
    public Guid Value { get; }

    private  BusinessServiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BusinessServiceId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static BusinessServiceId New() => new BusinessServiceId(Guid.NewGuid());
    
    public static BusinessServiceId From(Guid value) => new BusinessServiceId(value);
    
    public static implicit operator Guid(BusinessServiceId id) => id.Value;
    
    public override string ToString() => Value.ToString();
    
}