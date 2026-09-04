using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SendPulser.Webhooks;

namespace SendPulser.AspNetCore.Internal;

/// <summary>
/// Turns a webhook payload into typed events. SendPulse posts a JSON array of events, batched every
/// minute or every 100 events for the bulk email service and every 30 seconds or every 500 events for
/// the SMTP service.
/// </summary>
internal static class WebhookEventParser
{
    public static List<EmailWebhookEvent> ParseEmailEvents(JsonDocument document)
    {
        var events = new List<EmailWebhookEvent>();
        foreach (var element in EnumerateEvents(document))
        {
            events.Add(ParseEmailEvent(element));
        }

        return events;
    }

    public static List<SmtpWebhookEvent> ParseSmtpEvents(JsonDocument document)
    {
        var events = new List<SmtpWebhookEvent>();
        foreach (var element in EnumerateEvents(document))
        {
            events.Add(ParseSmtpEvent(element));
        }

        return events;
    }

    private static IEnumerable<JsonElement> EnumerateEvents(JsonDocument document)
    {
        var root = document.RootElement;

        // The documented shape is an array; a bare object is accepted so a single event still works.
        if (root.ValueKind is JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
            {
                if (element.ValueKind is JsonValueKind.Object)
                {
                    yield return element;
                }
            }
        }
        else if (root.ValueKind is JsonValueKind.Object)
        {
            yield return root;
        }
    }

    private static EmailWebhookEvent ParseEmailEvent(JsonElement element)
    {
        var context = WebhookJsonContext.Default;

        return GetEventName(element) switch
        {
            EmailWebhookEventNames.Delivered => Read(element, context.EmailDeliveredEvent),
            EmailWebhookEventNames.Open => Read(element, context.EmailOpenedEvent),
            EmailWebhookEventNames.Redirect => Read(element, context.EmailClickedEvent),
            EmailWebhookEventNames.Unsubscribe => Read(element, context.EmailUnsubscribedEvent),
            EmailWebhookEventNames.NewSubscriber => Read(element, context.EmailNewSubscriberEvent),
            EmailWebhookEventNames.Spam => Read(element, context.EmailSpamEvent),
            _ => Unknown(element),
        };

        static UnknownEmailEvent Unknown(JsonElement element)
        {
            var unknown = Read(element, WebhookJsonContext.Default.UnknownEmailEvent);
            unknown.Raw = element.Clone();
            return unknown;
        }
    }

    private static SmtpWebhookEvent ParseSmtpEvent(JsonElement element)
    {
        var context = WebhookJsonContext.Default;

        return GetEventName(element) switch
        {
            SmtpWebhookEventNames.Delivered => Read(element, context.SmtpDeliveredEvent),
            SmtpWebhookEventNames.Opened => Read(element, context.SmtpOpenedEvent),
            SmtpWebhookEventNames.Clicked => Read(element, context.SmtpClickedEvent),
            SmtpWebhookEventNames.Unsubscribed => Read(element, context.SmtpUnsubscribedEvent),
            SmtpWebhookEventNames.Resubscribed => Read(element, context.SmtpResubscribedEvent),
            _ => Unknown(element),
        };

        static UnknownSmtpEvent Unknown(JsonElement element)
        {
            var unknown = Read(element, WebhookJsonContext.Default.UnknownSmtpEvent);
            unknown.Raw = element.Clone();
            return unknown;
        }
    }

    private static string? GetEventName(JsonElement element) =>
        element.TryGetProperty("event", out var name) && name.ValueKind is JsonValueKind.String
            ? name.GetString()
            : null;

    private static T Read<T>(JsonElement element, JsonTypeInfo<T> typeInfo)
        where T : new() =>
        element.Deserialize(typeInfo) ?? new T();
}
