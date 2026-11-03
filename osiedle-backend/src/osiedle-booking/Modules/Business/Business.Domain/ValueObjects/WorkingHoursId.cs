namespace Business.Domain.ValueObjects;

public readonly record struct WorkingHoursId
{
    public Guid Value { get; }

    private WorkingHoursId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkingHoursId cannot be empty.", nameof(value));
        
        Value = value;
    }
    
    public static WorkingHoursId New() => new WorkingHoursId(Guid.NewGuid());
    
    public static WorkingHoursId From(Guid value) => new WorkingHoursId(value);
    
    public static implicit operator Guid(WorkingHoursId id) => id.Value;
    
    public override string ToString() => Value.ToString();
}
