using System.Text.RegularExpressions;

namespace Business.Domain.ValueObjects.Category;

public readonly partial record struct Slug
{
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    public string Value { get; }

    private Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(value));
        }

        value = value.Trim().ToLowerInvariant();
        
        if (value.Length > 100)
        {
            throw new ArgumentException("Value cannot exceed 100 characters.", nameof(value));
        }
        if (!SlugRegex().IsMatch(value))
        {
            throw new ArgumentException("Value is not a valid slug.", nameof(value));
        }

        Value = value;
    }
    
    public static Slug Create(string value) => new(value);
    
    public override string ToString() => Value ?? string.Empty;
}