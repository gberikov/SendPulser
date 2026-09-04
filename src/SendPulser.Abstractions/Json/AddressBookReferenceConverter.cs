using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SendPulser.EmailAddresses;

namespace SendPulser.Json;

/// <summary>
/// Reads a mailing list reference whose name SendPulse sends as <c>address_book_name</c> in one endpoint and
/// <c>name</c> in another.
/// </summary>
public sealed class AddressBookReferenceConverter : JsonConverter<AddressBookReference>
{
    /// <inheritdoc />
    public override AddressBookReference? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType is not JsonTokenType.StartObject)
        {
            throw new JsonException("A mailing list reference must be a JSON object.");
        }

        var reference = new AddressBookReference();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
        {
            if (reader.TokenType is not JsonTokenType.PropertyName)
            {
                continue;
            }

            var property = reader.GetString();
            reader.Read();
            switch (property)
            {
                case "id":
                    reference.Id = reader.TokenType is JsonTokenType.Number
                        ? reader.GetInt32()
                        : int.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
                    break;
                case "name":
                case "address_book_name":
                    reference.Name = reader.TokenType is JsonTokenType.String ? reader.GetString() : null;
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return reference;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, AddressBookReference value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();
        writer.WriteNumber("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteEndObject();
    }
}
