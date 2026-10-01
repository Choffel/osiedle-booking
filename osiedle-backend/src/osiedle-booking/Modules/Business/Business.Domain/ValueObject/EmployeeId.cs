namespace Business.Domain.ValueObject;

public readonly record struct EmployeeId
{
    public Guid Value { get; }

    private EmployeeId(Guid value) 
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static EmployeeId New() => new(Guid.NewGuid());
    public static EmployeeId From(Guid value) => new(value);
    
    public static implicit operator Guid(EmployeeId employeeId) => employeeId.Value;
    
    public override string ToString() => Value.ToString();
}