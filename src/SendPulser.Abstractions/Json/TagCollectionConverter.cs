using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Reads the template <c>tags</c> field, which SendPulse returns as an object (<c>{"webinar":"webinar"}</c>)
/// when tags exist and as an empty array when they do not.
/// </summary>
public sealed class TagCollectionConverter : JsonConverter<IReadOnlyList<string>>
{
    /// <inheritdoc />
    public override IReadOnlyList<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                var fromObject = new List<string>();
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
                {
                    if (reader.TokenType is not JsonTokenType.PropertyName)
                    {
                        continue;
                    }

                    var name = reader.GetString();
                    reader.Read();
                    var value = reader.TokenType is JsonTokenType.String ? reader.GetString() : null;
                    var tag = value ?? name;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        fromObject.Add(tag);
                    }
                }

                return fromObject;

            case JsonTokenType.StartArray:
                var fromArray = new List<string>();
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray)
                {
                    if (reader.TokenType is JsonTokenType.String && reader.GetString() is { Length: > 0 } item)
                    {
                        fromArray.Add(item);
                    }
                }

                return fromArray;

            default:
                reader.Skip();
                return [];
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IReadOnlyList<string> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartArray();
        foreach (var tag in value)
        {
            writer.WriteStringValue(tag);
        }

        writer.WriteEndArray();
    }
}
