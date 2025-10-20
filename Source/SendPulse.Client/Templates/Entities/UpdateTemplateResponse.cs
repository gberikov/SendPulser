using System.Text.Json.Serialization;

namespace SendPulse.Client.Templates.Entities;

public record UpdateTemplateResponse
{
    /// <summary>
    /// Result status (true if successful)
    /// </summary>
    [JsonPropertyName("result")]
    public bool Result { get; set; }
}