using System.Text.Json;
using System.Text.Json.Serialization;
using SendPulse.Client.Templates.Entities;

namespace SendPulse.Client.Templates.Converters;

/// <summary>
/// Custom JSON converter for CategoryInfo that can be either an object or an empty array
/// </summary>
public class CategoryInfoConverter : JsonConverter<CategoryInfo?>
{
    public override CategoryInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // If it's an empty array, skip it and return null
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
            }
            return null;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            // If it's an object, deserialize it
            return JsonSerializer.Deserialize<CategoryInfo>(ref reader, options);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, CategoryInfo? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteStartArray();
            writer.WriteEndArray();
        }
        else
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
