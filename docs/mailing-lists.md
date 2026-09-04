# Mailing lists and contacts

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

Two services cover contacts. `AddressBooks` works inside one mailing list; `EmailAddresses` looks an
address up across every mailing list of the account.

## Mailing lists (`client.AddressBooks`)

| Method | SendPulse endpoint |
| --- | --- |
| `GetAllAsync(limit, offset)` | `GET /addressbooks` |
| `GetAsync(id)` | `GET /addressbooks/{id}` |
| `CreateAsync(name)` | `POST /addressbooks` |
| `RenameAsync(id, name)` | `PUT /addressbooks/{id}` |
| `DeleteAsync(id)` | `DELETE /addressbooks/{id}` |
| `GetVariablesAsync(id)` | `GET /addressbooks/{id}/variables` |
| `GetCampaignCostAsync(id)` | `GET /addressbooks/{id}/cost` |
| `GetCampaignsAsync(id, limit, offset)` | `GET /addressbooks/{id}/campaigns` |

```csharp
var id = await client.AddressBooks.CreateAsync("Newsletter");
var cost = await client.AddressBooks.GetCampaignCostAsync(id);
if (!cost.IsAffordable) { /* top up before sending */ }
```

## Contacts in a mailing list

| Method | SendPulse endpoint |
| --- | --- |
| `GetContactsAsync(id, limit, offset, active, notActive)` | `GET /addressbooks/{id}/emails` |
| `GetContactAsync(id, email)` | `GET /addressbooks/{id}/emails/{email}` |
| `FindContactsByVariableAsync(id, name, value)` | `GET /addressbooks/{id}/variables/{name}/{value}` |
| `GetContactCountAsync(id)` | `GET /addressbooks/{id}/emails/total` |
| `AddContactsAsync(id, contacts, tagIds)` | `POST /addressbooks/{id}/emails` |
| `AddContactsWithConfirmationAsync(id, contacts, settings)` | `POST /addressbooks/{id}/emails` with `confirmation=force` |
| `UpdateVariablesAsync(id, email, variables)` | `POST /addressbooks/{id}/emails/variable` |
| `SetPhoneAsync(id, email, phone)` | `PUT /addressbooks/{id}/phone` |
| `UnsubscribeContactsAsync(id, emails)` | `POST /addressbooks/{id}/emails/unsubscribe` |
| `DeleteContactsAsync(id, emails)` | `DELETE /addressbooks/{id}/emails` |

SendPulse returns at most 100 contacts per call and deletes at most 100 per call; page with `limit`
and `offset`.

```csharp
await client.AddressBooks.AddContactsAsync(
    id,
    [
        new NewContact("ann@example.com") { Variables = new() { ["Name"] = "Ann", ["Phone"] = "+14783289866" } },
        new NewContact("bob@example.com"),
    ],
    tagIds: [3456]);

await client.AddressBooks.AddContactsWithConfirmationAsync(
    id,
    [new NewContact("carl@example.com")],
    new DoubleOptInSettings("hello@example.com", "en") { TemplateId = "a3e45169-..." });

await client.AddressBooks.UpdateVariablesAsync(id, "ann@example.com", [new("City", "Kyiv"), new("Joined", "2026-09-04")]);
```

Variable values are strings on the wire. Number variables accept only numbers, date variables only
`yyyy-MM-dd`. The phone number lives in the system variable `Phone`.

### Contact models

`Contact` is what the listing and search endpoints return. Its `Variables` dictionary holds every value
as text, whatever JSON type SendPulse used, so a variable typed as a number in one list and as a string
in another reads the same way. `ContactDetails`, returned by `GetContactAsync`, carries the typed
`ContactVariable` list SendPulse provides for a single contact (name, type and value).

`Contact.Status` is an integer; the codes are in `ContactStatus`:

| Code | Constant |
| --- | --- |
| 0 | `New` |
| 1 | `Active` |
| 2 | `ConfirmationRequested` |
| 3 | `ActivationRequested` |
| 4 | `Unsubscribed` |
| 5 | `RejectedByAdmin` |
| 6 | `UnsubscribedFromAll` |
| 7 | `ActivationEmailSent` |
| 8 | `BlockedByUser` |
| 9 | `SendingError` |
| 10 | `BlockedByHost` |
| 11 | `BlockedBySenderName` |
| 12 | `BlockedByAddressPart` |
| 13 | `DeletedByUser` |
| 14 | `TemporarilyUnavailable` |

`StatusExplanation` always carries the wording SendPulse itself returns, so an unlisted code is still
readable.

## Email addresses across lists (`client.EmailAddresses`)

| Method | SendPulse endpoint |
| --- | --- |
| `GetAsync(email)` | `GET /emails/{email}` |
| `GetDetailsAsync(email)` | `GET /emails/{email}/details` |
| `GetManyAsync(emails)` | `POST /emails` |
| `GetStatisticsAsync(email)` | `GET /emails/{email}/campaigns` |
| `GetStatisticsAsync(emails)` | `POST /emails/campaigns` |
| `DeleteFromAllListsAsync(email)` | `DELETE /emails/{email}` |

```csharp
var stats = await client.EmailAddresses.GetStatisticsAsync("ann@example.com");
Console.WriteLine($"{stats.Counters.Sent} sent, {stats.Counters.Opened} opened, blacklisted: {stats.IsBlacklisted}");
foreach (var book in stats.AddressBooks) Console.WriteLine($"{book.Id} {book.Name}");
```

The single and batch statistics endpoints return different shapes, so they map to
`EmailAddressStatistics` and `EmailAddressBatchStatistics`. Both expose the mailing lists as
`AddressBookReference`, although SendPulse names the list differently in each payload.
