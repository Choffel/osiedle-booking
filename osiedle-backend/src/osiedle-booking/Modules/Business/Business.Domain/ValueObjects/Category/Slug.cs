using System.Text.RegularExpressions;

namespace Business.Domain.ValueObjects.Category;

public readonly partial record struct Slug
{
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    public string Value { get; }

    private Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(value));
        }

        value = value.Trim().ToLowerInvariant();
        value = WhitespaceRegex().Replace(value, "-");
        
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
    public static Slug From(string value) => new(value);
    public override string ToString() => Value ?? string.Empty;
}