using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Reads the flags SendPulse returns as <c>true</c>, <c>1</c> or <c>"1"</c> depending on the endpoint and
/// exposes them as a <see cref="bool"/>. Writes a JSON boolean.
/// </summary>
public sealed class FlexibleBooleanConverter : JsonConverter<bool>
{
    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.TryGetInt64(out var number) ? number != 0 : reader.GetDouble() != 0,
            JsonTokenType.String => reader.GetString() is { } text
                && (text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase)),
            _ => false,
        };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteBooleanValue(value);
    }
}
