using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Writes binary attachments as the Base64 keyed object SendPulse expects, so callers hand over raw bytes.
/// </summary>
public sealed class Base64AttachmentsConverter : JsonConverter<Dictionary<string, byte[]>?>
{
    /// <inheritdoc />
    public override Dictionary<string, byte[]>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.StartObject)
        {
            reader.Skip();
            return null;
        }

        var result = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
        {
            if (reader.TokenType is not JsonTokenType.PropertyName)
            {
                continue;
            }

            var name = reader.GetString();
            reader.Read();
            if (name is null || reader.TokenType is not JsonTokenType.String)
            {
                continue;
            }

            var value = reader.GetString();
            if (value is not null && Convert.TryFromBase64String(value, new byte[value.Length], out _))
            {
                result[name] = Convert.FromBase64String(value);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Dictionary<string, byte[]>? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        foreach (var (name, content) in value)
        {
            writer.WriteString(name, Convert.ToBase64String(content));
        }

        writer.WriteEndObject();
    }
}
