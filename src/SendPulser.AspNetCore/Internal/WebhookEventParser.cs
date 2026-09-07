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
                if (element.ValueKind is not JsonValueKind.Object)
                {
                    throw new JsonException("Every SendPulse webhook batch item must be a JSON object.");
                }

                yield return element;
            }
        }
        else if (root.ValueKind is JsonValueKind.Object)
        {
            yield return root;
        }
        else
        {
            throw new JsonException("A SendPulse webhook payload must be an object or an array of objects.");
        }
    }

    private static EmailWebhookEvent ParseEmailEvent(JsonElement element)
    {
        var context = WebhookJsonContext.Default;

        try
        {
            return GetEventName(element) switch
            {
                EmailWebhookEventNames.Delivered => Read(element, context.EmailDeliveredEvent),
                EmailWebhookEventNames.Open => Read(element, context.EmailOpenedEvent),
                EmailWebhookEventNames.Redirect => Read(element, context.EmailClickedEvent),
                EmailWebhookEventNames.Unsubscribe => Read(element, context.EmailUnsubscribedEvent),
                EmailWebhookEventNames.NewSubscriber => Read(element, context.EmailNewSubscriberEvent),
                EmailWebhookEventNames.Delete => Read(element, context.EmailDeletedEvent),
                EmailWebhookEventNames.Spam => Read(element, context.EmailSpamEvent),
                EmailWebhookEventNames.TaskStatusUpdate => Read(element, context.EmailCampaignStatusEvent),
                EmailWebhookEventNames.SoftBounces => Read(element, context.EmailSoftBounceEvent),
                EmailWebhookEventNames.HardBounces => Read(element, context.EmailHardBounceEvent),
                _ => UnknownEmail(element),
            };
        }
        catch (Exception exception) when (exception is JsonException or ArgumentOutOfRangeException)
        {
            return UnknownEmail(element);
        }
    }

    private static SmtpWebhookEvent ParseSmtpEvent(JsonElement element)
    {
        var context = WebhookJsonContext.Default;

        try
        {
            return GetEventName(element) switch
            {
                SmtpWebhookEventNames.Delivered => Read(element, context.SmtpDeliveredEvent),
                SmtpWebhookEventNames.Undelivered => Read(element, context.SmtpUndeliveredEvent),
                SmtpWebhookEventNames.Opened => Read(element, context.SmtpOpenedEvent),
                SmtpWebhookEventNames.Clicked => Read(element, context.SmtpClickedEvent),
                SmtpWebhookEventNames.Unsubscribed => Read(element, context.SmtpUnsubscribedEvent),
                SmtpWebhookEventNames.Resubscribed => Read(element, context.SmtpResubscribedEvent),
                SmtpWebhookEventNames.Spam => Read(element, context.SmtpSpamEvent),
                SmtpWebhookEventNames.SoftBounces => Read(element, context.SmtpSoftBounceEvent),
                SmtpWebhookEventNames.HardBounces => Read(element, context.SmtpHardBounceEvent),
                _ => UnknownSmtp(element),
            };
        }
        catch (Exception exception) when (exception is JsonException or ArgumentOutOfRangeException)
        {
            return UnknownSmtp(element);
        }
    }

    private static string? GetEventName(JsonElement element) =>
        element.TryGetProperty("event", out var name) && name.ValueKind is JsonValueKind.String
            ? name.GetString()
            : null;

    private static T Read<T>(JsonElement element, JsonTypeInfo<T> typeInfo) =>
        element.Deserialize(typeInfo) ?? throw new JsonException("The SendPulse webhook event was empty.");

    private static UnknownEmailEvent UnknownEmail(JsonElement element) => new()
    {
        Event = GetEventName(element) ?? string.Empty,
        Email = GetString(element, "email"),
        TaskId = GetInt64(element, "task_id"),
        Timestamp = GetTimestamp(element),
        AdditionalProperties = GetAdditionalProperties(element, "event", "email", "task_id", "timestamp"),
        Raw = element.Clone(),
    };

    private static UnknownSmtpEvent UnknownSmtp(JsonElement element) => new()
    {
        Event = GetEventName(element) ?? string.Empty,
        MessageId = GetFlexibleString(element, "message_id"),
        Recipient = GetString(element, "recipient"),
        Sender = GetString(element, "sender"),
        Subject = GetString(element, "subject"),
        Timestamp = GetTimestamp(element),
        AdditionalProperties = GetAdditionalProperties(
            element, "event", "message_id", "recipient", "sender", "subject", "timestamp"),
        Raw = element.Clone(),
    };

    private static Dictionary<string, JsonElement>? GetAdditionalProperties(
        JsonElement element,
        params string[] knownProperties)
    {
        Dictionary<string, JsonElement>? additionalProperties = null;
        foreach (var property in element.EnumerateObject())
        {
            if (!knownProperties.Contains(property.Name, StringComparer.Ordinal))
            {
                additionalProperties ??= new(StringComparer.Ordinal);
                // Events outlive the request's JsonDocument, including these extension values.
                additionalProperties[property.Name] = property.Value.Clone();
            }
        }

        return additionalProperties;
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? GetFlexibleString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };
    }

    private static long? GetInt64(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt64(out var number) => number,
            JsonValueKind.String when long.TryParse(
                value.GetString(),
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out var number) => number,
            _ => null,
        };
    }

    private static DateTimeOffset? GetTimestamp(JsonElement element)
    {
        var seconds = GetInt64(element, "timestamp");
        if (seconds is null)
        {
            return null;
        }

        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds.Value);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
