using System.Text.Json;
using System.Text.Json.Serialization;
using SendPulse.Client.Common.Entities;

namespace SendPulse.Client.Templates.Converters;

/// <summary>
/// Custom JSON converter for tags that can be either an object (dictionary of tag objects) or an array
/// </summary>
public class TagConverter : JsonConverter<List<Tag>>
{
    public override List<Tag> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tags = new List<Tag>();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // If it's an array, deserialize each tag object
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var tag = JsonSerializer.Deserialize<Tag>(ref reader, options);
                    if (tag != null)
                        tags.Add(tag);
                }
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            // If it's an object (dictionary of tags), read each property as a tag
            // Example: {"valentine": "valentine"} or {"fooddelivery": "Food delivery"}
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var tagId = reader.GetString();
                    
                    // Read the property value (tag name)
                    reader.Read();
                    var tagName = reader.GetString();
                    
                    if (!string.IsNullOrEmpty(tagId))
                    {
                        tags.Add(new Tag 
                        { 
                            Id = tagId, 
                            Name = tagName ?? tagId 
                        });
                    }
                }
            }
        }

        return tags;
    }

    public override void Write(Utf8JsonWriter writer, List<Tag> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var tag in value)
        {
            writer.WriteString(tag.Id, tag.Name);
        }
        writer.WriteEndObject();
    }
}
