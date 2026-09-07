# SMTP (transactional email)

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

`client.Smtp` wraps the SMTP service API. The SMTP service must be activated on the account before any
call succeeds, and the sender address must be an activated sender, see
[Account](https://github.com/gberikov/SendPulser/blob/master/docs/account.md).

## Sending

```csharp
var result = await client.Smtp.SendAsync(new SendEmailRequest
{
    Subject = "Your order",
    From = new EmailAddress("orders@example.com", "Example"),
    To = [new EmailAddress("ann@example.com", "Ann")],
    Cc = [new EmailAddress("sales@example.com")],
    ReplyTo = new EmailAddress("support@example.com"),
    Html = "<p>Thanks for your order.</p>",    // plain HTML, Base64 encoded on the wire
    Text = "Thanks for your order.",
    Attachments = new() { ["invoice.txt"] = "plain text content" },
    BinaryAttachments = new() { ["invoice.pdf"] = pdfBytes },   // Base64 encoded on the wire
});

Console.WriteLine(result.Id);   // "pzkic9-0afezp-fc", use it to look the message up later
```

A stored template can replace the inline body:

```csharp
Template = new SmtpTemplateReference { Id = "123456", Variables = new() { ["name"] = "Ann" } }
```

`SendAsync` is not idempotent: a retry sends the message again. The resilience package therefore never
retries it; see
[Errors and resilience](https://github.com/gberikov/SendPulser/blob/master/docs/errors-and-resilience.md).

## Reading messages

| Method | SendPulse endpoint |
| --- | --- |
| `GetEmailsAsync(limit, offset, fromDate, toDate, sender, recipient)` | `GET /smtp/emails` |
| `GetEmailsAsync(includeCountry, limit, offset, fromDate, toDate, sender, recipient)` | `GET /smtp/emails`; `false` sends `country=off` |
| `GetEmailAsync(id)` | `GET /smtp/emails/{id}` |
| `GetEmailsAsync(ids)` | `POST /smtp/emails/info` (up to 500 IDs) |
| `GetTotalCountAsync()` | `GET /smtp/emails/total` |

`SmtpEmail` carries the SMTP answer of the receiving server and, when SendPulse recorded any, a
`Tracking` object with every open (`Clients`) and click (`Links`), including browser, operating system,
IP address and country.

## Bounces

| Method | SendPulse endpoint |
| --- | --- |
| `GetBouncesAsync(onDate, limit, offset)` | `GET /smtp/bounces/day` |
| `GetBounceCountAsync()` | `GET /smtp/bounces/day/total` |

SendPulse keeps bounce details for the last 24 hours only. For a permanent record, subscribe to the
`hard_bounces` and `soft_bounces` webhook events, see
[Webhooks](https://github.com/gberikov/SendPulser/blob/master/docs/webhooks.md).

## Unsubscribe list

| Method | SendPulse endpoint |
| --- | --- |
| `UnsubscribeAsync(requests)` | `POST /smtp/unsubscribe` |
| `RemoveFromUnsubscribeListAsync(emails)` | `DELETE /smtp/unsubscribe` |
| `GetUnsubscribedAsync(onDate, limit, offset)` | `GET /smtp/unsubscribe` |
| `IsUnsubscribedAsync(email)` | `GET /smtp/unsubscribe/search` |
| `ResubscribeAsync(email, sender, language)` | `POST /smtp/resubscribe` |

```csharp
await client.Smtp.UnsubscribeAsync([new UnsubscribeRequest("ann@example.com", "asked by phone")]);
if (await client.Smtp.IsUnsubscribedAsync("ann@example.com"))
    await client.Smtp.ResubscribeAsync("ann@example.com", "hello@example.com", "en");
```

`ResubscribeAsync` emails a confirmation request; SendPulse allows five per account per 24 hours.
`UnsubscribedContact` tells whether the contact left through a link, was removed by the account owner
or filed a spam complaint.

## Sending infrastructure

| Method | SendPulse endpoint |
| --- | --- |
| `GetSendersAsync()` | `GET /smtp/senders` |
| `GetIpAddressesAsync()` | `GET /smtp/ips` |
| `GetSenderDomainsAsync()` | `GET /v2/email-service/smtp/sender_domains` |
| `AddSenderDomainAsync(domain)` | `POST /v2/email-service/smtp/sender_domains/{domain}` |

`SenderDomain.Checks` reports the DKIM, SPF and DMARC validation and the exact SPF record SendPulse
expects to find. Adding a domain registers it; it activates once the DNS records are published.
