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

    /// <summary>The recipient opened the email.</summary>
    public const string Opened = "opened";

    /// <summary>The recipient clicked a link.</summary>
    public const string Clicked = "clicked";

    /// <summary>The recipient unsubscribed.</summary>
    public const string Unsubscribed = "unsubscribed";

    /// <summary>A previously unsubscribed recipient confirmed their subscription again.</summary>
    public const string Resubscribed = "resubscribed";
}

/// <summary>
/// Base type for events delivered by an SMTP service webhook.
/// </summary>
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
}

/// <summary>
/// The email was delivered, together with the response of the receiving server.
/// </summary>
public sealed class SmtpDeliveredEvent : SmtpWebhookEvent
{
    /// <summary>SMTP response code of the receiving server.</summary>
    [JsonPropertyName("smtp_server_response_code")]
    public int? ResponseCode { get; set; }

    /// <summary>SMTP enhanced status code.</summary>
    [JsonPropertyName("smtp_server_response_subcode")]
    public string? ResponseSubcode { get; set; }

    /// <summary>Raw response text of the receiving server.</summary>
    [JsonPropertyName("smtp_server_response")]
    public string? Response { get; set; }
}

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
/// An event whose name is not in <see cref="SmtpWebhookEventNames"/>, passed through untouched.
/// Bounce and spam events land here: SendPulse documents them in prose but never states their names.
/// </summary>
public sealed class UnknownSmtpEvent : SmtpWebhookEvent
{
    /// <summary>The untouched JSON object as it arrived.</summary>
    [JsonIgnore]
    public JsonElement Raw { get; set; }
}
