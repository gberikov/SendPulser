using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Serializes a list of IDs as a bare number when it holds exactly one item and as an array otherwise,
/// matching the <c>list_id</c> parameter SendPulse documents as "int/array". Reading accepts both shapes.
/// </summary>
public sealed class SingleOrArrayConverter : JsonConverter<IReadOnlyList<int>?>
{
    /// <inheritdoc />
    public override IReadOnlyList<int>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.Number:
                return [reader.GetInt32()];

            case JsonTokenType.String:
                return int.TryParse(reader.GetString(), out var single) ? [single] : null;

            case JsonTokenType.StartArray:
                var items = new List<int>();
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray)
                {
                    if (reader.TokenType is JsonTokenType.Number)
                    {
                        items.Add(reader.GetInt32());
                    }
                    else if (reader.TokenType is JsonTokenType.String && int.TryParse(reader.GetString(), out var item))
                    {
                        items.Add(item);
                    }
                }

                return items;

            default:
                reader.Skip();
                return null;
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IReadOnlyList<int>? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Count == 1)
        {
            writer.WriteNumberValue(value[0]);
            return;
        }

        writer.WriteStartArray();
        foreach (var id in value)
        {
            writer.WriteNumberValue(id);
        }

        writer.WriteEndArray();
    }
}
