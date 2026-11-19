namespace Files.Domain.ValueObjects;

public readonly record struct FileId
{
    public Guid Value { get; }

    public FileId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FileId cannot be empty.", nameof(value));

        Value = value;
    }

    public static FileId New() => new(Guid.NewGuid());

    public static FileId From(Guid value) => new(value);

    public static FileId? FromNullable(Guid? value)
    {
        return value.HasValue && value.Value != Guid.Empty
            ? new FileId(value.Value)
            : null;
    }

    public static implicit operator Guid(FileId fileId) => fileId.Value;

    public override string ToString() => Value.ToString();
}
