using System.Text.Json.Serialization;

namespace SendPulse.Client.Templates.Entities;

public record CreateTemplateResponse
{
    /// <summary>
    /// Result status (true if successful)
    /// </summary>
    [JsonPropertyName("result")]
    public bool Result { get; set; }

    /// <summary>
    /// Real numeric ID
    /// </summary>
    [JsonPropertyName("real_id")]
    public int RealId { get; set; }
}