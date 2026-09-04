# Webhooks

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

Two parts: `client.Webhooks` registers webhooks through the API, and the `SendPulser.AspNetCore`
package maps the endpoints that receive the events.

## Registering (`client.Webhooks`)

| Method | SendPulse endpoint |
| --- | --- |
| `GetAllAsync()` | `GET /v2/email-service/webhook` |
| `GetAsync(id)` | `GET /v2/email-service/webhook/{id}` |
| `CreateAsync(url, actions)` | `POST /v2/email-service/webhook` |
| `UpdateAsync(id, url)` | `PUT /v2/email-service/webhook/{id}` |
| `DeleteAsync(id)` | `DELETE /v2/email-service/webhook/{id}` |

SendPulse stores one row per event, so subscribing a URL to three events yields three `Webhook` rows
sharing the URL. The event names accepted as `actions` are the constants of `EmailWebhookEventNames`;
`EmailWebhookEventNames.All` subscribes to everything SendPulse documents.

```csharp
var rows = await client.Webhooks.CreateAsync(
    "https://example.com/hooks/sendpulse/email/" + secret,
    [EmailWebhookEventNames.Delivered, EmailWebhookEventNames.HardBounces, EmailWebhookEventNames.Spam]);
```

SMTP webhooks are configured in the SendPulse account settings; the API registers bulk email webhooks
only.

## Receiving

```csharp
app.MapSendPulserEmailWebhook(
    "/hooks/sendpulse/email/{secret}",
    async (events, cancellationToken) =>
    {
        foreach (var received in events)
            await store.RecordAsync(received, cancellationToken);
    },
    options => options.Secret = builder.Configuration["SendPulser:WebhookSecret"]);

app.MapSendPulserSmtpWebhook(
    "/hooks/sendpulse/smtp/{secret}",
    async (events, context, cancellationToken) =>
    {
        // This overload hands over the HttpContext, so scoped services can be resolved.
        var db = context.RequestServices.GetRequiredService<AppDbContext>();
        ...
    },
    options => options.Secret = builder.Configuration["SendPulser:WebhookSecret"]);
```

The endpoint answers 204 when the callback returns, 500 when it throws, 400 for a body that is not
JSON, 413 for a body above `MaxRequestBodySize` (1 MiB by default) and 404 when the secret does not
match.

### Security

SendPulse signs nothing: no HMAC, no shared header, no documented source addresses. The options offer
what is possible:

- `Secret`: a random string that must appear in the URL. It is read from the `{secret}` route segment
  and, as a fallback, from a `secret` query parameter; prefer the route segment, query strings end up in
  more access logs. The comparison is constant time. Serve the endpoint over HTTPS.
- `AllowedAddresses`: an optional allowlist of source addresses. Behind a proxy this needs forwarded
  headers to be handled first.
- `MaxRequestBodySize`: caps the body size before it is parsed.

### Batches

Events arrive in batches: up to 100 per minute for the bulk email service and up to 500 every 30
seconds for the SMTP service. The whole batch is handed to the callback at once. A callback that fails
half way answers 500, and SendPulse may or may not resend; deduplicate on the receiving side if that
matters.

## Event types

The bulk email service and the SMTP service post different payloads with different event names, so
they map to two type hierarchies. Every documented event has a class; an event with a name the library
does not know arrives as `UnknownEmailEvent` or `UnknownSmtpEvent` with the untouched JSON in `Raw`.
On top of that, every event keeps fields it has no property for in `AdditionalProperties`, so a field
SendPulse adds later is never lost.

### Bulk email (`EmailWebhookEvent`)

Common fields: `Event`, `Timestamp`, `TaskId` (the campaign), `Email`.

| `event` | Class | Extra fields |
| --- | --- | --- |
| `delivered` | `EmailDeliveredEvent` | |
| `open` | `EmailOpenedEvent` | `Device`, `Platform`, `BrowserName`, `BrowserVersion` |
| `redirect` | `EmailClickedEvent` | the open fields plus `LinkUrl`, `LinkId` |
| `unsubscribe` | `EmailUnsubscribedEvent` | `FromAll`, `Reason`, `BookId`, `Categories` |
| `new_emails` | `EmailNewSubscriberEvent` | `BookId`, `Source`, `Variables` |
| `delete` | `EmailDeletedEvent` | `BookId` |
| `spam` | `EmailSpamEvent` | `Source`, `AutomationId` |
| `task_status_update` | `EmailCampaignStatusEvent` | `Status`, `StatusExplanation`, `BookId` |
| `soft_bounces` | `EmailSoftBounceEvent` | `ResponseCode`, `ResponseSubcode`, `Response` |
| `hard_bounces` | `EmailHardBounceEvent` | `ResponseCode`, `ResponseSubcode`, `Response` |
| anything else | `UnknownEmailEvent` | `Raw` |

`EmailCampaignStatusEvent.Status` is one of `approve`, `approve_part`, `only_active`,
`confirm_addresses`, `need_edit`, `rejected`, `on_moderation`, `sending`.

### SMTP (`SmtpWebhookEvent`)

Common fields: `Event`, `Timestamp`, `MessageId`, `Recipient`, `Sender`, `Subject`.

| `event` | Class | Extra fields |
| --- | --- | --- |
| `delivered` | `SmtpDeliveredEvent` | `ResponseCode`, `ResponseSubcode`, `Response` |
| `undelivered` | `SmtpUndeliveredEvent` | `ResponseCode`, `ResponseSubcode`, `Response` |
| `opened` | `SmtpOpenedEvent` | |
| `clicked` | `SmtpClickedEvent` | |
| `unsubscribed` | `SmtpUnsubscribedEvent` | |
| `resubscribed` | `SmtpResubscribedEvent` | |
| `spam_by_user` | `SmtpSpamEvent` | |
| `soft_bounces` | `SmtpSoftBounceEvent` | the response fields plus `Email`, `TaskId` |
| `hard_bounces` | `SmtpHardBounceEvent` | the response fields plus `Email`, `TaskId` |
| anything else | `UnknownSmtpEvent` | `Raw` |

SendPulse documents the SMTP bounce payloads with the field names of the bulk email service (`email`
and `task_id` instead of `recipient` and `message_id`), which is why the bounce events expose both
sets.

```csharp
foreach (var received in events)
{
    switch (received)
    {
        case SmtpHardBounceEvent bounce:
            await suppress(bounce.Email ?? bounce.Recipient);
            break;
        case UnknownSmtpEvent unknown:
            logger.LogInformation("New SendPulse event {Event}: {Raw}", unknown.Event, unknown.Raw);
            break;
    }
}
```
