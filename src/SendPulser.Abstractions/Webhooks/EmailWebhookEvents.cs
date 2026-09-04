using System.Text.Json;
using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Webhooks;

/// <summary>
/// Event names sent by the bulk email service. SendPulse does not publish the full catalogue, so any
/// name not listed here is surfaced as <see cref="UnknownEmailEvent"/> rather than rejected.
/// </summary>
public static class EmailWebhookEventNames
{
    /// <summary>The email reached the recipient's server.</summary>
    public const string Delivered = "delivered";

    /// <summary>The recipient opened the email.</summary>
    public const string Open = "open";

    /// <summary>The recipient clicked a link.</summary>
    public const string Redirect = "redirect";

    /// <summary>The recipient unsubscribed.</summary>
    public const string Unsubscribe = "unsubscribe";

    /// <summary>A contact was added to a mailing list.</summary>
    public const string NewSubscriber = "new_emails";

    /// <summary>The recipient marked the email as spam.</summary>
    public const string Spam = "spam";
}

/// <summary>
/// Base type for events delivered by a bulk email service webhook.
/// </summary>
public abstract class EmailWebhookEvent
{
    /// <summary>Raw event name as sent by SendPulse.</summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>When the event happened.</summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(UnixTimestampConverter))]
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>ID of the campaign the event belongs to.</summary>
    [JsonPropertyName("task_id")]
    public long? TaskId { get; set; }

    /// <summary>Recipient address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>
/// The email was delivered.
/// </summary>
public sealed class EmailDeliveredEvent : EmailWebhookEvent;

/// <summary>
/// The recipient opened the email.
/// </summary>
public class EmailOpenedEvent : EmailWebhookEvent
{
    /// <summary>Device the email was opened on.</summary>
    [JsonPropertyName("open_device")]
    public string? Device { get; set; }

    /// <summary>Operating system the email was opened on.</summary>
    [JsonPropertyName("open_platform")]
    public string? Platform { get; set; }

    /// <summary>Browser name.</summary>
    [JsonPropertyName("browser_name")]
    public string? BrowserName { get; set; }

    /// <summary>Browser version.</summary>
    [JsonPropertyName("browser_ver")]
    public string? BrowserVersion { get; set; }
}

/// <summary>
/// The recipient clicked a link in the email.
/// </summary>
public sealed class EmailClickedEvent : EmailOpenedEvent
{
    /// <summary>URL that was clicked.</summary>
    [JsonPropertyName("link_url")]
    public string? LinkUrl { get; set; }

    /// <summary>ID of the clicked link.</summary>
    [JsonPropertyName("link_id")]
    public long? LinkId { get; set; }
}

/// <summary>
/// The recipient unsubscribed.
/// </summary>
public sealed class EmailUnsubscribedEvent : EmailWebhookEvent
{
    /// <summary>Non-zero when the recipient unsubscribed from every mailing list of the sender.</summary>
    [JsonPropertyName("from_all")]
    public int? FromAll { get; set; }

    /// <summary>Reason given by the recipient, when any.</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>Mailing list the recipient unsubscribed from.</summary>
    [JsonPropertyName("book_id")]
    public long? BookId { get; set; }

    /// <summary>Categories the recipient unsubscribed from.</summary>
    [JsonPropertyName("categories")]
    public string? Categories { get; set; }
}

/// <summary>
/// A contact was added to a mailing list.
/// </summary>
public sealed class EmailNewSubscriberEvent : EmailWebhookEvent
{
    /// <summary>Mailing list the contact was added to.</summary>
    [JsonPropertyName("book_id")]
    public long? BookId { get; set; }

    /// <summary>How the contact was collected, for example a subscription form.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

/// <summary>
/// The recipient marked the email as spam.
/// </summary>
public sealed class EmailSpamEvent : EmailWebhookEvent
{
    /// <summary>Where the email originated, for example <c>automation360</c>.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>Automation flow the email belonged to.</summary>
    [JsonPropertyName("automation_id")]
    public string? AutomationId { get; set; }
}

/// <summary>
/// An event whose name is not in <see cref="EmailWebhookEventNames"/>. SendPulse publishes neither a
/// complete event catalogue nor a payload schema, so unrecognised events are passed through untouched
/// instead of failing the request.
/// </summary>
public sealed class UnknownEmailEvent : EmailWebhookEvent
{
    /// <summary>The untouched JSON object as it arrived.</summary>
    [JsonIgnore]
    public JsonElement Raw { get; set; }
}
