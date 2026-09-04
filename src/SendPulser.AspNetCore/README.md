# SendPulser.AspNetCore

ASP.NET Core endpoints that receive SendPulse webhooks as typed events, for
[SendPulser](https://github.com/gberikov/SendPulser).

```csharp
app.MapSendPulserEmailWebhook(
    "/hooks/sendpulse/{secret}",
    async (events, cancellationToken) => await store.RecordAsync(events, cancellationToken),
    options => options.Secret = configuration["SendPulser:WebhookSecret"]);
```

SendPulse signs nothing, so a secret in the URL compared in constant time is the authentication; an
address allowlist and a body size limit are available on top. Events arrive in batches and are handed
over as one list. Every documented event of the bulk email and SMTP services has a class; unknown
events keep their original JSON, and unknown fields on known events are preserved.

Documentation:
[Webhooks](https://github.com/gberikov/SendPulser/blob/master/docs/webhooks.md).
