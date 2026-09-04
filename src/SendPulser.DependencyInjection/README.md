# SendPulser.DependencyInjection

Dependency injection and `IHttpClientFactory` wiring for
[SendPulser](https://github.com/gberikov/SendPulser).

```csharp
builder.Services.AddSendPulser(builder.Configuration.GetSection(SendPulserOptions.SectionName));
```

Registers `ISendPulserClient` as a singleton together with the OAuth token provider and the
authentication handler, and returns the `IHttpClientBuilder` so policies can be added on top, for
example with `SendPulser.Resilience`. Configuration is read key by key, so the package stays usable
under Native AOT.

Documentation:
[Getting started](https://github.com/gberikov/SendPulser/blob/master/docs/getting-started.md),
[Configuration](https://github.com/gberikov/SendPulser/blob/master/docs/configuration.md).
