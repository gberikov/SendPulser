using System.Text.Json.Serialization;
using SendPulser.Webhooks;

namespace SendPulser.AspNetCore.Internal;

/// <summary>
/// Source generated metadata for the webhook payloads, keeping the endpoints trimming and AOT safe.
/// </summary>
[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(EmailDeliveredEvent))]
[JsonSerializable(typeof(EmailOpenedEvent))]
[JsonSerializable(typeof(EmailClickedEvent))]
[JsonSerializable(typeof(EmailUnsubscribedEvent))]
[JsonSerializable(typeof(EmailNewSubscriberEvent))]
[JsonSerializable(typeof(EmailDeletedEvent))]
[JsonSerializable(typeof(EmailSpamEvent))]
[JsonSerializable(typeof(EmailCampaignStatusEvent))]
[JsonSerializable(typeof(EmailSoftBounceEvent))]
[JsonSerializable(typeof(EmailHardBounceEvent))]
[JsonSerializable(typeof(UnknownEmailEvent))]
[JsonSerializable(typeof(SmtpDeliveredEvent))]
[JsonSerializable(typeof(SmtpUndeliveredEvent))]
[JsonSerializable(typeof(SmtpOpenedEvent))]
[JsonSerializable(typeof(SmtpClickedEvent))]
[JsonSerializable(typeof(SmtpUnsubscribedEvent))]
[JsonSerializable(typeof(SmtpResubscribedEvent))]
[JsonSerializable(typeof(SmtpSpamEvent))]
[JsonSerializable(typeof(SmtpSoftBounceEvent))]
[JsonSerializable(typeof(SmtpHardBounceEvent))]
[JsonSerializable(typeof(UnknownSmtpEvent))]
internal sealed partial class WebhookJsonContext : JsonSerializerContext;
