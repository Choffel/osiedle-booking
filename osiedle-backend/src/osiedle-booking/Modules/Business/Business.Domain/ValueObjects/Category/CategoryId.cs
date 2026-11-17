namespace Business.Domain.ValueObjects.Category;

public readonly record struct CategoryId
{
    public Guid Value { get; }

    public CategoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CategoryId cannot be empty.", nameof(value));
        
        Value = value;
    }

    public static CategoryId New() => new CategoryId(Guid.NewGuid());
    
    public static CategoryId From(Guid value) => new(value);
    
    public static CategoryId? FromNullable(Guid? value)
    {
        return value.HasValue && value.Value != Guid.Empty
            ? new CategoryId(value.Value)
            : null;
    }
    
    public static implicit operator Guid(CategoryId categoryId) => categoryId.Value;
    
    public override string ToString() => Value.ToString();
}