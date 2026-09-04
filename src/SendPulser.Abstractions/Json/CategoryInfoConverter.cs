using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SendPulser.Templates;

namespace SendPulser.Json;

/// <summary>
/// Reads the template <c>category_info</c> field, which SendPulse returns as an empty array when the
/// template has no category and as an object when it does.
/// </summary>
public sealed class CategoryInfoConverter : JsonConverter<TemplateCategory?>
{
    /// <inheritdoc />
    public override TemplateCategory? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (reader.TokenType is not JsonTokenType.StartObject)
        {
            reader.Skip();
            return null;
        }

        var typeInfo = (JsonTypeInfo<TemplateCategory>)options.GetTypeInfo(typeof(TemplateCategory));
        return JsonSerializer.Deserialize(ref reader, typeInfo);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TemplateCategory? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var typeInfo = (JsonTypeInfo<TemplateCategory>)options.GetTypeInfo(typeof(TemplateCategory));
        JsonSerializer.Serialize(writer, value, typeInfo);
    }
}
