using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Reads and writes the <c>yyyy-MM-dd HH:mm:ss</c> timestamps SendPulse returns instead of ISO-8601.
/// Empty strings, <c>null</c> and unparsable values become <see langword="null"/>.
/// </summary>
/// <remarks>
/// SendPulse states these values in the time zone of the account, without an offset, so string values
/// are returned with <see cref="DateTimeKind.Unspecified"/> and written without conversion. The rare
/// numeric value is a Unix timestamp and is returned as UTC.
/// </remarks>
public sealed class SendPulseDateTimeConverter : JsonConverter<DateTime?>
{
    internal const string Format = "yyyy-MM-dd HH:mm:ss";

    /// <inheritdoc />
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        string? value = null;
        if (reader.TokenType is JsonTokenType.String)
        {
            value = reader.GetString();
        }
        else if (reader.TokenType is JsonTokenType.Number && reader.TryGetInt64(out var unix))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            return exact;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var loose)
            ? loose
            : null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
