# Getting started

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

## Install

Applications with a service container:

```bash
dotnet add package SendPulser.DependencyInjection
dotnet add package SendPulser.Resilience      # retries, rate limiting, timeouts
dotnet add package SendPulser.AspNetCore      # webhook endpoints
```

Scripts and console tools can reference `SendPulser` alone. Code that only needs the contracts, for
example a domain project that is unit tested with a mock, references `SendPulser.Abstractions`.

All five packages share one version and target `net8.0` and `net10.0`.

## Register the client

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

The credentials are the OAuth client ID and secret from the API tab of the SendPulse account settings.
Keep them in user secrets or environment variables, never in a committed `appsettings.json`.

`ISendPulserClient` is registered as a singleton and is safe to inject anywhere, including hosted
services. Every option is described in
[Configuration](https://github.com/gberikov/SendPulser/blob/master/docs/configuration.md).

## First request

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

The sender address must be activated in the account first, see
[Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md).

## Without a container

```csharp
using var client = new SendPulserClient(clientId, clientSecret);
var books = await client.AddressBooks.GetAllAsync(limit: 20);
```

This client owns its `HttpClient` and must be disposed. It has no rate limiter and no retries; add
them yourself or use the container registration when the process makes more than a handful of calls.

## Services on the client

| Property | Interface | Documentation |
| --- | --- | --- |
| `AddressBooks` | `IAddressBookService` | [Mailing lists and contacts](https://github.com/gberikov/SendPulser/blob/master/docs/mailing-lists.md) |
| `EmailAddresses` | `IEmailAddressService` | [Mailing lists and contacts](https://github.com/gberikov/SendPulser/blob/master/docs/mailing-lists.md) |
| `Campaigns` | `ICampaignService` | [Campaigns](https://github.com/gberikov/SendPulser/blob/master/docs/campaigns.md) |
| `Templates` | `ITemplateService` | [Templates](https://github.com/gberikov/SendPulser/blob/master/docs/templates.md) |
| `Senders` | `ISenderService` | [Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md) |
| `Blacklist` | `IBlacklistService` | [Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md) |
| `Tags` | `ITagService` | [Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md) |
| `Balance` | `IBalanceService` | [Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md) |
| `Smtp` | `ISmtpService` | [SMTP](https://github.com/gberikov/SendPulser/blob/master/docs/smtp.md) |
| `Webhooks` | `IWebhookService` | [Webhooks](https://github.com/gberikov/SendPulser/blob/master/docs/webhooks.md) |

## Conventions

- Every method takes a `CancellationToken` as its last parameter.
- Paging is passed through exactly as SendPulse defines it: `limit` and `offset`. No method issues
  hidden extra requests.
- Responses are typed models. Values SendPulse returns inconsistently (numbers as strings, flags as
  `0`/`1`, tags as an object or an array, timestamps without a time zone) are normalised by converters
  in `SendPulser.Json`.
- Timestamps are returned as `DateTime` with `DateTimeKind.Unspecified` because SendPulse states them in
  the time zone of the account. Webhook timestamps are Unix seconds and arrive as `DateTimeOffset`.
- Trimming and Native AOT are supported; nothing is serialized through reflection.
