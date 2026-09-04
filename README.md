# SendPulser

A modern .NET 8 client for the [SendPulse](https://sendpulse.com/integrations/api) email API: mailing
lists, contacts, templates, campaigns, transactional email and webhooks.

Trimming and Native AOT compatible, no reflection-based serialization, no runtime dependencies in the
core package beyond `Microsoft.Extensions.Logging.Abstractions`.

## Packages

| Package | What it gives you |
| --- | --- |
| `SendPulser.Abstractions` | Interfaces, models, options and exceptions. Reference this from your domain code and mock everything. |
| `SendPulser` | The client itself: HTTP pipeline, OAuth handling, service implementations. |
| `SendPulser.DependencyInjection` | `AddSendPulser(...)` on `IServiceCollection`, wired through `IHttpClientFactory`. |
| `SendPulser.Resilience` | Retry, client-side rate limiting and timeouts tuned to the SendPulse quotas. |
| `SendPulser.AspNetCore` | Endpoints that receive SendPulse webhooks. |

All five ship together under one version.

## Getting started

```bash
dotnet add package SendPulser.DependencyInjection
```

```csharp
builder.Services
    .AddSendPulser(builder.Configuration.GetSection(SendPulserOptions.SectionName))
    .AddSendPulserResilience();
```

```json
{
  "SendPulser": {
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

```csharp
public sealed class WelcomeMailer(ISendPulserClient sendPulse)
{
    public Task SendAsync(string email, CancellationToken cancellationToken) =>
        sendPulse.Smtp.SendAsync(
            new SendEmailRequest
            {
                Subject = "Welcome",
                Html = "<p>Glad to have you.</p>",
                From = new EmailAddress("hello@example.com", "Example"),
                To = [new EmailAddress(email)],
            },
            cancellationToken);
}
```

Without a container, build the client yourself and dispose it:

```csharp
using var client = new SendPulserClient(clientId, clientSecret);
var books = await client.AddressBooks.GetAllAsync(limit: 20);
```

## What is covered

- **Mailing lists** — create, rename, delete, list, variables, contacts, single and double opt-in
  imports, contact removal.
- **Templates** — list, read, create, edit. HTML bodies are passed as plain markup and Base64 encoded
  on the wire.
- **Campaigns** — list, read with per status statistics, create, edit scheduled, cancel.
- **SMTP** — send, list, read, totals, unsubscribe list management, sender addresses.
- **Webhooks** — registration through the API, plus endpoints that receive the events.

Paging is passed through exactly as SendPulse defines it: `limit` and `offset`, no hidden extra
requests. SendPulse returns at most 100 contacts per call.

## Errors

Every failure is a `SendPulserApiException` carrying the status code, the SendPulse error code and the
raw body. This includes failures SendPulse reports with HTTP 200 and an `is_error` payload, which the
client detects rather than returning a half-empty object.

`SendPulserAuthenticationException` is raised when the credentials are refused.

## Rate limits and retries

SendPulse enforces a hard ceiling of **10 requests per second** (proprietary error code `2020202020`)
plus a per-minute quota that depends on the plan (1000 / 2000 / 3000), answered with HTTP 429.

`AddSendPulserResilience()` adds a client-side limiter for the per-second ceiling, an attempt timeout
and retries. **Writes are never retried.** Sending a campaign or a transactional email is not
idempotent, so a replayed `POST` can deliver the same email twice; only `GET` and `DELETE` are replayed
on transient failures. HTTP 429 is retried for every method, honouring `Retry-After`, because the
request was rejected rather than processed.

## Webhooks

```csharp
app.MapSendPulserEmailWebhook(
    "/hooks/sendpulse/{secret}",
    async (events, cancellationToken) =>
    {
        foreach (var received in events)
        {
            await store.RecordAsync(received, cancellationToken);
        }
    },
    options => options.Secret = builder.Configuration["SendPulser:WebhookSecret"]);
```

Things worth knowing before you rely on them:

- **SendPulse signs nothing.** No HMAC, no shared header, no documented source addresses. A secret in
  the URL, compared in constant time, is the only authentication available; serve the endpoint over
  HTTPS. An optional address allowlist is available through `options.AllowedAddresses`.
- **Events arrive in batches**, up to 100 per minute for the bulk email service and up to 500 every
  30 seconds for SMTP. The whole batch is handed to your callback at once.
- **The two services use different payloads.** The bulk email service sends `event: "open"` with
  `task_id` and `email`; the SMTP service sends `event: "opened"` with `message_id`, `recipient` and
  `sender`. They are modelled as two separate type hierarchies, mapped by
  `MapSendPulserEmailWebhook` and `MapSendPulserSmtpWebhook`.
- **SendPulse publishes no event catalogue and no payload schema.** Anything unrecognised arrives as
  `UnknownEmailEvent` / `UnknownSmtpEvent` with the original JSON in `Raw`, so a new event type never
  breaks a running deployment. Bounce and spam events land there today.
- The endpoint answers 204 when your callback returns, 500 when it throws, 400 for a body that is not
  JSON and 404 when the secret does not match.

## Compatibility

Targets `net8.0`. `netstandard2.0` is deliberately not supported.

## Samples

- `samples/SendPulser.Sample.Console` — credentials from the environment, lists mailing lists and
  templates.
- `samples/SendPulser.Sample.Web` — dependency injection, resilience and both webhook endpoints.

## Working on the library

```bash
dotnet build SendPulser.slnx
dotnet test tests/SendPulser.Tests/SendPulser.Tests.csproj -p:CollectCoverage=true -p:Threshold=80
```

The build treats warnings as errors and tracks the public API surface with
`Microsoft.CodeAnalysis.PublicApiAnalyzers`: changing anything public fails the build until the change
is recorded in the `PublicAPI.Unshipped.txt` file of that package. Rider and Visual Studio offer an
"Add to public API" quick fix (with fix-all) for exactly this.

Coverage is measured over the implementation packages; `SendPulser.Abstractions` is excluded because
it holds only serialization models and interfaces, where a percentage would only measure how many
property tests were written to raise it.

## License

MIT.
