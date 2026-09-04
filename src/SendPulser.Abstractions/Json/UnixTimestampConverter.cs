using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Reads the Unix second timestamps carried by webhook payloads. SendPulse sends them both as numbers
/// and as strings, sometimes within the same event family.
/// </summary>
public sealed class UnixTimestampConverter : JsonConverter<DateTimeOffset?>
{
    /// <inheritdoc />
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number when reader.TryGetInt64(out var number):
                return DateTimeOffset.FromUnixTimeSeconds(number);
            case JsonTokenType.String:
                var text = reader.GetString();
                return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                    ? DateTimeOffset.FromUnixTimeSeconds(parsed)
                    : null;
            default:
                return null;
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(value.Value.ToUnixTimeSeconds());
    }
}
