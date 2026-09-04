# SendPulser

A .NET client for the [SendPulse](https://sendpulse.com/integrations/api) email API: mailing lists and
contacts, campaigns, templates, senders, blacklist, tags, balance, transactional email over SMTP and
webhook registrations. Targets `net8.0` and `net10.0`; trimming and Native AOT compatible.

```csharp
using var client = new SendPulserClient(clientId, clientSecret);

var books = await client.AddressBooks.GetAllAsync(limit: 20);
await client.Smtp.SendAsync(new SendEmailRequest
{
    Subject = "Welcome",
    Html = "<p>Glad to have you.</p>",
    From = new EmailAddress("hello@example.com", "Example"),
    To = [new EmailAddress("someone@example.com")],
});
```

In an application with a service container, install `SendPulser.DependencyInjection` instead and call
`services.AddSendPulser(...)` so the client runs on a pooled `IHttpClientFactory` handler.

Documentation:

- [Getting started](https://github.com/gberikov/SendPulser/blob/master/docs/getting-started.md)
- [Configuration](https://github.com/gberikov/SendPulser/blob/master/docs/configuration.md)
- [Errors and resilience](https://github.com/gberikov/SendPulser/blob/master/docs/errors-and-resilience.md)
- [Mailing lists and contacts](https://github.com/gberikov/SendPulser/blob/master/docs/mailing-lists.md),
  [Campaigns](https://github.com/gberikov/SendPulser/blob/master/docs/campaigns.md),
  [Templates](https://github.com/gberikov/SendPulser/blob/master/docs/templates.md),
  [Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md),
  [SMTP](https://github.com/gberikov/SendPulser/blob/master/docs/smtp.md),
  [Webhooks](https://github.com/gberikov/SendPulser/blob/master/docs/webhooks.md)
