using System.Text.Json;
using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Webhooks;

/// <summary>
/// Event names sent by the bulk email service. They double as the <c>actions</c> accepted by
/// <see cref="IWebhookService.CreateAsync"/>. A name not listed here is surfaced as
/// <see cref="UnknownEmailEvent"/> rather than rejected.
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

    /// <summary>A contact was removed from a mailing list.</summary>
    public const string Delete = "delete";

    /// <summary>The recipient marked the email as spam.</summary>
    public const string Spam = "spam";

    /// <summary>The status of a campaign changed.</summary>
    public const string TaskStatusUpdate = "task_status_update";

    /// <summary>The receiving server rejected the email temporarily.</summary>
    public const string SoftBounces = "soft_bounces";

    /// <summary>The receiving server rejected the email permanently.</summary>
    public const string HardBounces = "hard_bounces";

    /// <summary>Every name SendPulse documents, in the form <see cref="IWebhookService.CreateAsync"/> accepts.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Delivered, Open, Redirect, Unsubscribe, NewSubscriber, Delete, Spam, TaskStatusUpdate, SoftBounces, HardBounces,
    ];
}

/// <summary>
/// Base type for events delivered by a bulk email service webhook.
/// </summary>
/// <remarks>
/// SendPulse publishes no payload schema. Every documented field is typed; anything else in the payload
/// is kept in <see cref="AdditionalProperties"/> so a field SendPulse adds later is not lost.
/// </remarks>
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

    /// <summary>Fields of the payload that have no typed property on this event.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
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
    /// <summary>Whether the recipient unsubscribed from every mailing list of the sender.</summary>
    [JsonPropertyName("from_all")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool FromAll { get; set; }

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

    /// <summary>How the contact was collected, for example <c>subscription form</c> or <c>address book</c>.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>Variables the contact was added with, empty when none.</summary>
    [JsonPropertyName("variables")]
    [JsonConverter(typeof(VariableMapConverter))]
    public Dictionary<string, string?>? Variables { get; set; }
}

/// <summary>
/// A contact was removed from a mailing list.
/// </summary>
public sealed class EmailDeletedEvent : EmailWebhookEvent
{
    /// <summary>Mailing list the contact was removed from.</summary>
    [JsonPropertyName("book_id")]
    public long? BookId { get; set; }
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
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? AutomationId { get; set; }
}

/// <summary>
/// The status of a campaign changed, for example after moderation.
/// </summary>
public sealed class EmailCampaignStatusEvent : EmailWebhookEvent
{
    /// <summary>
    /// New status: <c>approve</c>, <c>approve_part</c>, <c>only_active</c>, <c>confirm_addresses</c>,
    /// <c>need_edit</c>, <c>rejected</c>, <c>on_moderation</c> or <c>sending</c>.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Human readable status, as sent by SendPulse.</summary>
    [JsonPropertyName("status_explain")]
    public string? StatusExplanation { get; set; }

    /// <summary>Mailing list the campaign goes to.</summary>
    [JsonPropertyName("book_id")]
    public long? BookId { get; set; }
}

/// <summary>
/// The receiving server rejected the email. See the sealed subclasses for the kind of rejection.
/// </summary>
public abstract class EmailBounceEvent : EmailWebhookEvent
{
    /// <summary>SMTP response code of the receiving server.</summary>
    [JsonPropertyName("smtp_server_response_code")]
    public int? ResponseCode { get; set; }

    /// <summary>SMTP enhanced status code.</summary>
    [JsonPropertyName("smtp_server_response_subcode")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? ResponseSubcode { get; set; }

    /// <summary>Raw response text of the receiving server.</summary>
    [JsonPropertyName("smtp_server_response")]
    public string? Response { get; set; }
}

/// <summary>
/// The receiving server rejected the email temporarily, for example because the mailbox is full.
/// </summary>
public sealed class EmailSoftBounceEvent : EmailBounceEvent;

/// <summary>
/// The receiving server rejected the email permanently, for example because the address does not exist.
/// </summary>
public sealed class EmailHardBounceEvent : EmailBounceEvent;

/// <summary>
/// An event whose name is not in <see cref="EmailWebhookEventNames"/>. It is passed through untouched
/// instead of failing the request, so a new event type never breaks a running deployment.
/// </summary>
public sealed class UnknownEmailEvent : EmailWebhookEvent
{
    /// <summary>The untouched JSON object as it arrived.</summary>
    [JsonIgnore]
    public JsonElement Raw { get; set; }
}
