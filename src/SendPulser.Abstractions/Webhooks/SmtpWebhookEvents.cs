using System.Text.Json;
using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Webhooks;

/// <summary>
/// Event names sent by the SMTP (transactional) service. They differ from the bulk email names:
/// the same action is <c>open</c> there and <c>opened</c> here.
/// </summary>
public static class SmtpWebhookEventNames
{
    /// <summary>The email reached the recipient's server.</summary>
    public const string Delivered = "delivered";

    /// <summary>The receiving server refused the email.</summary>
    public const string Undelivered = "undelivered";

    /// <summary>The recipient opened the email.</summary>
    public const string Opened = "opened";

    /// <summary>The recipient clicked a link.</summary>
    public const string Clicked = "clicked";

    /// <summary>The recipient unsubscribed.</summary>
    public const string Unsubscribed = "unsubscribed";

    /// <summary>A previously unsubscribed recipient confirmed their subscription again.</summary>
    public const string Resubscribed = "resubscribed";

    /// <summary>The recipient marked the email as spam.</summary>
    public const string Spam = "spam_by_user";

    /// <summary>The receiving server rejected the email temporarily.</summary>
    public const string SoftBounces = "soft_bounces";

    /// <summary>The receiving server rejected the email permanently.</summary>
    public const string HardBounces = "hard_bounces";
}

/// <summary>
/// Base type for events delivered by an SMTP service webhook.
/// </summary>
/// <remarks>
/// SendPulse publishes no payload schema. Every documented field is typed; anything else in the payload
/// is kept in <see cref="AdditionalProperties"/> so a field SendPulse adds later is not lost.
/// </remarks>
public abstract class SmtpWebhookEvent
{
    /// <summary>Raw event name as sent by SendPulse.</summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>When the event happened.</summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(UnixTimestampConverter))]
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>ID of the message, as returned when it was sent.</summary>
    [JsonPropertyName("message_id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? MessageId { get; set; }

    /// <summary>Recipient address.</summary>
    [JsonPropertyName("recipient")]
    public string? Recipient { get; set; }

    /// <summary>Sender address.</summary>
    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    /// <summary>Email subject.</summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    /// <summary>Fields of the payload that have no typed property on this event.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

/// <summary>
/// The receiving server answered the delivery attempt. See the sealed subclasses for the outcome.
/// </summary>
public abstract class SmtpDeliveryResultEvent : SmtpWebhookEvent
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
/// The email was delivered.
/// </summary>
public sealed class SmtpDeliveredEvent : SmtpDeliveryResultEvent;

/// <summary>
/// The receiving server refused the email.
/// </summary>
public sealed class SmtpUndeliveredEvent : SmtpDeliveryResultEvent;

/// <summary>
/// The recipient opened the email.
/// </summary>
public sealed class SmtpOpenedEvent : SmtpWebhookEvent;

/// <summary>
/// The recipient clicked a link in the email.
/// </summary>
public sealed class SmtpClickedEvent : SmtpWebhookEvent;

/// <summary>
/// The recipient unsubscribed.
/// </summary>
public sealed class SmtpUnsubscribedEvent : SmtpWebhookEvent;

/// <summary>
/// A previously unsubscribed recipient confirmed their subscription again.
/// </summary>
public sealed class SmtpResubscribedEvent : SmtpWebhookEvent;

/// <summary>
/// The recipient marked the email as spam and was unsubscribed.
/// </summary>
public sealed class SmtpSpamEvent : SmtpWebhookEvent;

/// <summary>
/// The receiving server rejected the email. SendPulse documents these payloads with the field names of
/// the bulk email service, so the address arrives in <see cref="Email"/> and the campaign in
/// <see cref="TaskId"/> rather than in <see cref="SmtpWebhookEvent.Recipient"/>.
/// </summary>
public abstract class SmtpBounceEvent : SmtpDeliveryResultEvent
{
    /// <summary>Recipient address, as the bounce payload names it.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>Campaign or task ID, as the bounce payload names it.</summary>
    [JsonPropertyName("task_id")]
    public long? TaskId { get; set; }
}

/// <summary>
/// The receiving server rejected the email temporarily, for example because the mailbox is full.
/// </summary>
public sealed class SmtpSoftBounceEvent : SmtpBounceEvent;

/// <summary>
/// The receiving server rejected the email permanently, for example because the address does not exist.
/// </summary>
public sealed class SmtpHardBounceEvent : SmtpBounceEvent;

/// <summary>
/// An event whose name is not in <see cref="SmtpWebhookEventNames"/>, passed through untouched so a new
/// event type never breaks a running deployment.
/// </summary>
public sealed class UnknownSmtpEvent : SmtpWebhookEvent
{
    /// <summary>The untouched JSON object as it arrived.</summary>
    [JsonIgnore]
    public JsonElement Raw { get; set; }
}
