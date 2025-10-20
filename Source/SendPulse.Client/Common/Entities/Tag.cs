using System.Text.Json.Serialization;

namespace SendPulse.Client.Common.Entities;

/// <summary>
/// Represents a tag for an email template
/// </summary>
public class Tag
{
    /// <summary>
    /// Tag ID (e.g., "fooddelivery")
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tag name (e.g., "Food delivery")
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

