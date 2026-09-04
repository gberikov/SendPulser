# Configuration

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

## Options

`SendPulserOptions` is the single options type. `AddSendPulser(IConfiguration)` reads it key by key,
without reflection, so the package stays usable under Native AOT.

| Property | Configuration key | Default | Notes |
| --- | --- | --- | --- |
| `ClientId` | `ClientId` | required | OAuth client ID |
| `ClientSecret` | `ClientSecret` | required | OAuth client secret |
| `BaseAddress` | `BaseAddress` (alias `BaseUrl`) | `https://api.sendpulse.com/` | Must be HTTPS unless it points at the local machine |
| `Timeout` | `Timeout` | `00:01:40` | `HttpClient.Timeout` of both the API and the token client |
| `TokenRefreshMargin` | `TokenRefreshMargin` | `00:01:00` | Tokens are refreshed this long before they expire |

`SendPulserOptions.Validate()` runs on startup through `PostConfigure` and throws `ArgumentException`
for a missing credential, a relative or plain `http://` base address, a negative margin or a
non-positive timeout.

```csharp
builder.Services.AddSendPulser(options =>
{
    options.ClientId = "...";
    options.ClientSecret = "...";
    options.TokenRefreshMargin = TimeSpan.FromMinutes(5);
});
```

## What gets registered

| Service | Lifetime | Purpose |
| --- | --- | --- |
| `ISendPulserClient` | singleton | Entry point |
| `SendPulserTokenProvider` | singleton | Requests and caches the OAuth token, refreshes it under a lock |
| `SendPulserAuthenticationHandler` | transient | Adds the bearer token, replays a request once after a 401 |
| named `HttpClient` `"SendPulser"` | factory | API calls; returned as `IHttpClientBuilder` so policies can be added |
| named `HttpClient` `"SendPulser.Token"` | factory | Token requests; bypasses the authentication handler |

Both named clients use an infinite handler lifetime with a `SocketsHttpHandler` whose
`PooledConnectionLifetime` is five minutes. That is what makes the singleton client safe: connection
recycling is handled by the socket handler, not by rotating `HttpClient` instances.

To add your own handlers or policies, use the returned builder:

```csharp
builder.Services
    .AddSendPulser(configuration.GetSection("SendPulser"))
    .AddHttpMessageHandler<MyAuditHandler>()
    .AddSendPulserResilience();
```

## Tokens

SendPulse tokens live for one hour and several may be valid at once. The provider caches one token per
process and refreshes it one minute before expiry. Concurrent callers wait for a single refresh. When
SendPulse answers 401 to an API call, the handler invalidates the token it used, obtains a fresh one
and replays the request once with the original body. A token that another caller already replaced is
not thrown away.

`TimeProvider` is resolved from the container when registered, which makes expiry testable.

## Logging

The client logs through `ILogger` when a logger factory is registered:

| Event | Level | Category |
| --- | --- | --- |
| Token obtained | Debug | `SendPulser.SendPulserTokenProvider` |
| Token rejected, refreshing | Information | `SendPulser.SendPulserAuthenticationHandler` |
| Webhook batch handled | Debug | `SendPulser.Webhooks` |
| Webhook request rejected | Warning | `SendPulser.Webhooks` |
| Webhook body not JSON | Warning | `SendPulser.Webhooks` |
| Webhook handler threw | Error | `SendPulser.Webhooks` |

Request and response logging is left to `IHttpClientFactory`, which already logs both at Information
level under `System.Net.Http.HttpClient.SendPulser`. Those lines include the full request URI, and
SendPulse query strings carry email addresses; lower that category to Warning in production if that
matters to you. The client's own log lines never include the query string, the token or the secret.

## Building the client by hand

```csharp
var options = new SendPulserOptions { ClientId = "...", ClientSecret = "..." };
using var client = new SendPulserClient(options);
```

For full control, build the pipeline yourself; the `HttpClient` must carry the base address:

```csharp
var tokenClient = new HttpClient { BaseAddress = new Uri(SendPulserOptions.DefaultBaseUrl) };
var tokens = new SendPulserTokenProvider(tokenClient, options);
var apiClient = new HttpClient(new SendPulserAuthenticationHandler(tokens) { InnerHandler = new SocketsHttpHandler() })
{
    BaseAddress = new Uri(SendPulserOptions.DefaultBaseUrl),
};
var client = new SendPulserClient(apiClient);
```
