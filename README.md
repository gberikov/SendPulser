# SendPulser

A .NET client for the [SendPulse](https://sendpulse.com/integrations/api) email API: mailing lists and
contacts, campaigns, templates, senders, blacklist, tags, balance, transactional email over SMTP and
webhooks, with typed models for every documented payload.

Targets `net8.0` and `net10.0`. Trimming and Native AOT compatible: no reflection-based serialization,
no runtime dependencies in the core package beyond `Microsoft.Extensions.Logging.Abstractions`.

## Packages

| Package | What it gives you |
| --- | --- |
| `SendPulser.Abstractions` | Interfaces, models, options and exceptions. Reference this from your domain code and mock everything. |
| `SendPulser` | The client itself: HTTP pipeline, OAuth handling, service implementations. |
| `SendPulser.DependencyInjection` | `AddSendPulser(...)` on `IServiceCollection`, wired through `IHttpClientFactory`. |
| `SendPulser.Resilience` | Retry, client-side rate limiting and timeouts tuned to the SendPulse quotas. |
| `SendPulser.AspNetCore` | Endpoints that receive SendPulse webhooks as typed events. |

All five ship together under one version.

## Quick start

```bash
dotnet add package SendPulser.DependencyInjection
```

```csharp
builder.Services
    .AddSendPulser(builder.Configuration.GetSection(SendPulserOptions.SectionName))
    .AddSendPulserResilience();
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

## Documentation

| Page | Covers |
| --- | --- |
| [Getting started](https://github.com/gberikov/SendPulser/blob/master/docs/getting-started.md) | Installing, registering the client, first request, conventions |
| [Configuration](https://github.com/gberikov/SendPulser/blob/master/docs/configuration.md) | Options, configuration keys, HTTP pipeline, tokens, logging |
| [Errors and resilience](https://github.com/gberikov/SendPulser/blob/master/docs/errors-and-resilience.md) | Exception types, rate limits, retries, timeouts |
| [Mailing lists and contacts](https://github.com/gberikov/SendPulser/blob/master/docs/mailing-lists.md) | Address books, contacts, variables, email address lookups |
| [Campaigns](https://github.com/gberikov/SendPulser/blob/master/docs/campaigns.md) | Creating, scheduling, statistics, status codes |
| [Templates](https://github.com/gberikov/SendPulser/blob/master/docs/templates.md) | Listing, reading, creating, editing |
| [Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md) | Senders, blacklist, tags, balance |
| [SMTP](https://github.com/gberikov/SendPulser/blob/master/docs/smtp.md) | Transactional email, bounces, unsubscribe list, sender domains |
| [Webhooks](https://github.com/gberikov/SendPulser/blob/master/docs/webhooks.md) | Registering webhooks, receiving events, every event type |
| [Releasing](https://github.com/gberikov/SendPulser/blob/master/docs/releasing.md) | Versioning, tags, public API files, NuGet |

## Coverage

Every endpoint of the SendPulse bulk email service and SMTP service documentation is wrapped, with the
exception of the `country` display flag of the SMTP message list. Responses are typed; values SendPulse
returns inconsistently (numbers as strings, flags as `0`/`1`, tags as an object or an array,
timestamps without a time zone) are normalised. Webhook events are typed per event name, and unknown
events or unknown fields are preserved rather than dropped.

## Samples

- `samples/SendPulser.Sample.Console`: credentials from the environment, prints balance, senders,
  mailing lists and templates.
- `samples/SendPulser.Sample.Web`: dependency injection, resilience and both webhook endpoints.
  Copy `appsettings.example.json` to user secrets.

## Working on the library

See [CONTRIBUTING.md](https://github.com/gberikov/SendPulser/blob/master/CONTRIBUTING.md) for the
build rules and [SECURITY.md](https://github.com/gberikov/SendPulser/blob/master/SECURITY.md) for
reporting vulnerabilities.

## License

MIT.
