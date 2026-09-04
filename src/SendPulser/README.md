# SendPulser

A modern .NET 8 client for the [SendPulse](https://sendpulse.com/integrations/api) email API: mailing
lists, contacts, templates, campaigns and transactional email.

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

Trimming and Native AOT compatible. Full documentation:
https://github.com/gberikov/SendPulser
