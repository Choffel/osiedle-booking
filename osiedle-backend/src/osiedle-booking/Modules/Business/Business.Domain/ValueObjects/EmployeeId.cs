namespace Business.Domain.ValueObjects;

public readonly record struct EmployeeId
{
    public Guid Value { get; }

    public EmployeeId(Guid value) 
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static EmployeeId New() => new(Guid.NewGuid());

    public static EmployeeId From(Guid value) => new(value);
    
    public static EmployeeId? FromNullable(Guid? value)
    {
        return value.HasValue && value.Value != Guid.Empty
            ? new EmployeeId(value.Value)
            : null;
    }
    
    public static implicit operator Guid(EmployeeId employeeId) => employeeId.Value;

    public static explicit operator EmployeeId(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}