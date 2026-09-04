using System.Text.Json.Serialization;

namespace SendPulser.Webhooks;

/// <summary>
/// A webhook registration stored in the SendPulse account. SendPulse keeps one row per event, so
/// subscribing to three events yields three rows sharing the same URL.
/// </summary>
public sealed class Webhook
{
    /// <summary>Webhook ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>ID of the account that owns the webhook.</summary>
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    /// <summary>HTTPS URL events are posted to.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>Event this row subscribes to.</summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
}
