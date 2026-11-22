namespace Files.Domain.ValueObjects;

public readonly record struct ContentType
{
    private static readonly HashSet<string> SupportedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    public string Value { get; }

    public ContentType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ContentType cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized == "image/jpg")
            normalized = "image/jpeg";

        if (!SupportedImageTypes.Contains(normalized))
            throw new ArgumentException($"ContentType '{value}' is not a supported image format.", nameof(value));

        Value = normalized;
    }

    public static ContentType Jpeg => new("image/jpeg");
    public static ContentType Png => new("image/png");
    public static ContentType Webp => new("image/webp");
    public static ContentType Gif => new("image/gif");

    public static ContentType From(string value) => new(value);

    public static bool IsSupported(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        var normalized = contentType.Trim().ToLowerInvariant();
        return SupportedImageTypes.Contains(normalized);
    }

    public static string GetExtension(string contentType)
    {
        var normalized = contentType.Trim().ToLowerInvariant();
        return normalized switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".bin"
        };
    }

    public static implicit operator string(ContentType contentType) => contentType.Value;

    public override string ToString() => Value;
}
