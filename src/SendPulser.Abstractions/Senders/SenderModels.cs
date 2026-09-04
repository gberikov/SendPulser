using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Senders;

/// <summary>
/// A sender address registered in the account.
/// </summary>
public sealed class Sender
{
    /// <summary>Sender email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Sender display name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Status as reported by SendPulse, for example <c>Active</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Whether the address may also be used by the SMTP service. SendPulse merged the sender lists of the
    /// two services on 2023-05-17; addresses added since then carry <see langword="true"/>.
    /// </summary>
    [JsonPropertyName("is_allowed_for_smtp")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsAllowedForSmtp { get; set; }
}
