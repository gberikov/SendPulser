using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulse.Client.Common.Converters;

/// <summary>
/// Custom JSON converter for DateTime that handles SendPulse date format: "yyyy-MM-dd HH:mm:ss" (UTC)
/// </summary>
public class SendPulseDateTimeConverter : JsonConverter<DateTime?>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();

            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            // Try to parse with SendPulse format and treat as UTC
            if (DateTime.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
            {
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            // Fallback to standard parsing
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var fallbackDate))
            {
                return DateTime.SpecifyKind(fallbackDate, DateTimeKind.Utc);
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            var utcValue = value.Value.Kind == DateTimeKind.Utc 
                ? value.Value 
                : value.Value.ToUniversalTime();
            
            writer.WriteStringValue(utcValue.ToString(DateFormat, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
