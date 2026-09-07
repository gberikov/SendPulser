using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Reads contact variables returned as an object (<c>{"name":"John","code":42}</c>) and exposes every
/// value as a string, whatever JSON type SendPulse chose for it. An empty array, which SendPulse sends when
/// a contact has no variables, becomes an empty dictionary.
/// </summary>
public sealed class VariableMapConverter : JsonConverter<Dictionary<string, string?>?>
{
    /// <inheritdoc />
    public override Dictionary<string, string?>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.StartObject:
                var result = new Dictionary<string, string?>(StringComparer.Ordinal);
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
                {
                    if (reader.TokenType is not JsonTokenType.PropertyName)
                    {
                        continue;
                    }

                    var name = reader.GetString()!;
                    reader.Read();
                    result[name] = ReadScalar(ref reader);
                }

                return result;

            default:
                reader.Skip();
                return new Dictionary<string, string?>(StringComparer.Ordinal);
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Dictionary<string, string?>? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        foreach (var (name, text) in value)
        {
            writer.WriteString(name, text);
        }

        writer.WriteEndObject();
    }

    private static string? ReadScalar(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                using (var document = JsonDocument.ParseValue(ref reader))
                {
                    return document.RootElement.GetRawText();
                }
            case JsonTokenType.True:
                return "true";
            case JsonTokenType.False:
                return "false";
            case JsonTokenType.Null:
                return null;
            default:
                // Nested structures are not variables SendPulse documents; keep the raw JSON text.
                using (var document = JsonDocument.ParseValue(ref reader))
                {
                    return document.RootElement.GetRawText();
                }
        }
    }
}
