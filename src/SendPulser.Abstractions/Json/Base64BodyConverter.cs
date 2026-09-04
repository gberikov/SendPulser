using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SendPulser.Json;

/// <summary>
/// Transparently Base64-encodes HTML bodies on the way out and decodes them on the way in, so callers
/// pass plain markup while SendPulse receives what it requires.
/// </summary>
public sealed class Base64BodyConverter : JsonConverter<string?>
{
    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // SendPulse returns the body plain on reads and expects it Base64 on writes, so decoding is best effort.
        Span<byte> buffer = value.Length <= 4096 ? stackalloc byte[value.Length] : new byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out var written)
            ? Encoding.UTF8.GetString(buffer[..written])
            : value;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
    }
}
