# Account: senders, blacklist, tags, balance

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

## Senders (`client.Senders`)

Campaigns and transactional emails can only be sent from an activated sender address. SendPulse merged
the sender lists of the bulk email and SMTP services on 2023-05-17; addresses added since then are
usable in both and carry `IsAllowedForSmtp = true`.

| Method | SendPulse endpoint |
| --- | --- |
| `GetAllAsync()` | `GET /senders` |
| `AddAsync(email, name)` | `POST /senders` |
| `DeleteAsync(email)` | `DELETE /senders` |
| `RequestActivationCodeAsync(email)` | `GET /senders/{email}/code` |
| `ActivateAsync(email, code)` | `POST /senders/{email}/code` |

```csharp
await client.Senders.AddAsync("hello@example.com", "Example");
// SendPulse emails an activation code to hello@example.com; one code per 15 minutes.
await client.Senders.ActivateAsync("hello@example.com", codeFromTheEmail);
```

Free mailbox providers are refused (error codes 20 and 97).

## Blacklist (`client.Blacklist`)

Addresses on the blacklist never receive campaigns, whatever mailing list they are in.

| Method | SendPulse endpoint |
| --- | --- |
| `GetAllAsync()` | `GET /blacklist` |
| `AddAsync(emails, comment)` | `POST /blacklist` |
| `RemoveAsync(emails)` | `DELETE /blacklist` |

SendPulse expects the addresses as one comma separated, Base64 encoded string; the service does the
encoding.

```csharp
await client.Blacklist.AddAsync(["spam@example.com", "bounced@example.com"], "imported from the old system");
```

## Tags (`client.Tags`)

Tags require the Pro plan or above. SendPulse queues every tag write and answers with an
acknowledgement rather than the changed tag; the acknowledgement is returned as `TagOperationResult`,
and a request that was not queued raises `SendPulserApiException`.

| Method | SendPulse endpoint |
| --- | --- |
| `GetAllAsync()` | `GET /tags` |
| `CreateAsync(name, color)` | `POST /tags` |
| `UpdateAsync(id, name, color)` | `PUT /tags/{id}` |
| `DeleteAsync(id)` | `DELETE /tags/{id}` |
| `AssignToEmailAsync(email, tagIds)` | `POST /tags/pin/email` |
| `AssignToPhoneAsync(phone, tagIds)` | `POST /tags/pin/phone` |
| `UnassignFromEmailAsync(email, tagIds)` | `POST /tags/unpin/email` |
| `UnassignFromPhoneAsync(phone, tagIds)` | `POST /tags/unpin/phone` |

```csharp
var queued = await client.Tags.CreateAsync("vip", "#f0f4f6");
Console.WriteLine(queued.QueueId);

var tags = await client.Tags.GetAllAsync();      // the new tag appears once the queue is processed
await client.Tags.AssignToEmailAsync("ann@example.com", [tags[0].Id]);
```

Tags can also be attached while adding contacts, through the `tagIds` parameter of
`IAddressBookService.AddContactsAsync`.

## Balance (`client.Balance`)

| Method | SendPulse endpoint |
| --- | --- |
| `GetAsync(currency)` | `GET /balance` or `GET /balance/{currency}` |
| `GetDetailsAsync()` | `GET /user/balance/detail` |

```csharp
var balance = await client.Balance.GetAsync("USD");
var details = await client.Balance.GetDetailsAsync();
Console.WriteLine($"{details.Email?.EmailsLeft} emails left on {details.Email?.Name}");
```

`BalanceDetails` has one section per service the account uses: `Email`, `Smtp` and `Push`; a section
is `null` when the service is not active.
