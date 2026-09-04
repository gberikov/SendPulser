# SendPulser.Resilience

Retry, rate limiting and timeout policies for [SendPulser](https://github.com/gberikov/SendPulser),
tuned to the limits SendPulse enforces: a hard ceiling of 10 requests per second plus a per-minute
quota answered with HTTP 429.

```csharp
builder.Services
    .AddSendPulser(builder.Configuration.GetSection(SendPulserOptions.SectionName))
    .AddSendPulserResilience();
```

Writes are never retried: sending a campaign or a transactional email is not idempotent, so only `GET`
and `DELETE` are replayed on transient failures. HTTP 429 is retried for every method, honouring
`Retry-After`. When the pipeline gives up, the failure surfaces as `SendPulserTransportException`.

Documentation:
[Errors and resilience](https://github.com/gberikov/SendPulser/blob/master/docs/errors-and-resilience.md).
