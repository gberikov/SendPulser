# SendPulser.AspNetCore

ASP.NET Core endpoints that receive SendPulse webhooks, for
[SendPulser](https://github.com/gberikov/SendPulser).

```csharp
app.MapSendPulserEmailWebhook(
    "/hooks/sendpulse/{secret}",
    async (events, cancellationToken) => await store.RecordAsync(events, cancellationToken),
    options => options.Secret = configuration["SendPulser:WebhookSecret"]);
```

SendPulse signs nothing, so a secret in the URL compared in constant time is the authentication;
an address allowlist is available on top. Events arrive in batches and are handed over as one list.
Unknown event types are surfaced with their original JSON instead of failing the request.
