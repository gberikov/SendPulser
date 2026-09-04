# SendPulser.DependencyInjection

Dependency injection and `IHttpClientFactory` wiring for
[SendPulser](https://github.com/gberikov/SendPulser).

```csharp
builder.Services.AddSendPulser(builder.Configuration.GetSection(SendPulserOptions.SectionName));
```

Registers `ISendPulserClient`, the OAuth token provider and the authentication handler, and returns the
`IHttpClientBuilder` so policies can be added on top, for example with `SendPulser.Resilience`.
