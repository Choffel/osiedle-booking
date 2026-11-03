using System.Text.Json;
using System.Text.Json.Serialization;

namespace osiedle_booking.WebApi.Json;

/// <summary>
/// Serializes GUID-based value objects as plain GUID strings instead of objects like { "value": "..." }.
/// </summary>
public sealed class GuidValueObjectJsonConverter<T> : JsonConverter<T> where T : struct
{
    private readonly Func<Guid, T> _fromGuid;
    private readonly Func<T, Guid> _toGuid;

    public GuidValueObjectJsonConverter(Func<Guid, T> fromGuid, Func<T, Guid> toGuid)
    {
        _fromGuid = fromGuid;
        _toGuid = toGuid;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a GUID string for {typeof(T).Name}.");
        }

        var raw = reader.GetString();

        if (!Guid.TryParse(raw, out var value) || value == Guid.Empty)
        {
            throw new JsonException($"Invalid GUID value for {typeof(T).Name}.");
        }

        return _fromGuid(value);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_toGuid(value));
    }
}
