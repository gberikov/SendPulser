# Campaigns

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

`client.Campaigns` wraps the bulk email campaign endpoints. SendPulse accepts at most four campaigns
per hour through the API (error code 791).

| Method | SendPulse endpoint |
| --- | --- |
| `GetAllAsync(limit, offset, order, statuses, scheduled)` | `GET /campaigns` |
| `GetAsync(id)` | `GET /campaigns/{id}` |
| `GetCountryStatisticsAsync(id)` | `GET /campaigns/{id}/countries` |
| `GetReferralStatisticsAsync(id)` | `GET /campaigns/{id}/referrals` |
| `GetRecipientAsync(id, email)` | `GET /campaigns/{id}/email/{email}` |
| `CreateAsync(request)` | `POST /campaigns` |
| `UpdateAsync(id, request)` | `PATCH /campaigns/{id}` |
| `CancelAsync(id)` | `DELETE /campaigns/{id}` |

## Creating

```csharp
var result = await client.Campaigns.CreateAsync(new CreateCampaignRequest
{
    SenderName = "Example",
    SenderEmail = "hello@example.com",     // must be an activated sender
    Subject = "September news",
    Body = "<h1>Hello</h1>",               // plain HTML, Base64 encoded on the wire
    ListIds = [756589],                    // up to ten lists; one for test or segmented campaigns
    SendDate = new DateTime(2026, 9, 10, 9, 0, 0),
});
```

Either `Body` or `TemplateId` is required, and either `ListIds` or `SegmentId`. Without `SendDate`
and without `Type = "draft"`, sending starts immediately. `IsTest = true` sends to the sender address
only. `UseDynamicList = true` includes contacts added to the list between scheduling and sending.

`SendDate` is interpreted by SendPulse in the time zone of the account, not in UTC, and must not be in
the past (error code 799). Pass the wall clock time you see in the SendPulse interface; the library does
no conversion.

The result carries the campaign ID and the initial status: 13 while addresses are being copied, 26 for
a draft.

`CreateCampaignRequest` also supports `BinaryAttachments`, an `AmpBody` and open/click `Statistics`.
Set `Statistics.UtmCampaign` to add a custom UTM campaign value; it is sent inside `stats`.
Binary attachments and the AMP body are Base64 encoded on the wire.

## Reading

`GetAllAsync` returns `Campaign` with aggregated counters; `GetAsync` returns `CampaignInfo` with the
message body, tracking settings, per status counters and per link clicks.

```csharp
var info = await client.Campaigns.GetAsync(14973974);
foreach (var row in info.Statistics!.General)
    Console.WriteLine($"{row.Code} {row.Explanation}: {row.Count}");

var byCountry = await client.Campaigns.GetCountryStatisticsAsync(14973974);   // {"US": 23, "UA": 34567}
var recipient = await client.Campaigns.GetRecipientAsync(14973974, "ann@example.com");
```

`GetAllAsync` filters: `order` is `asc` or `desc`, `statuses` restricts by status code, `scheduled`
includes scheduled campaigns.

## Status codes

`Campaign.Status` and `CampaignInfo.Status` use the codes in `CampaignStatus`:

| Code | Constant | Code | Constant |
| --- | --- | --- | --- |
| 0 | `New` | 14 | `Queued` |
| 1 | `InModeration` | 15 | `AwaitingAbTest` |
| 2 | `Sending` | 16 | `Cancelled` |
| 3 | `Sent` | 22 | `SendingPartially` |
| 4 | `Test` | 23 | `SentPartially` |
| 5 | `Blocked` | 25 | `PartiallySentAndBlocked` |
| 6 | `MarkedForDeletion` | 26 | `Draft` |
| 7 | `StatusUpdating` | 27 | `RequiresEditing` |
| 8 | `TestSent` | 28 | `ResendToUnreadScheduled` |
| 9 | `Delivering` | 33 | `AutomationBalanceExceeded` |
| 10 | `Preparing` | 36 | `AutomationDraft` |
| 11 | `AwaitingModeratorAnswer` | | |
| 12 | `NoActiveAddresses` | | |
| 13 | `Creating` | | |

Per recipient delivery codes, used in `CampaignStatistics.General` and `CampaignRecipient`, are in
`DeliveryStatus` (0 in queue, 1 sent, 2 delivered, 3 opened, 4 clicked, 5 unsubscribed, 6 no such
email, 7 temporarily unavailable, 8 unavailable, 9 rejected as spam, 10 mailbox full, 11 marked as
spam, 12 delivery failure, 16 invalid email, 17 temporarily blocked, 18 disabled by administrator,
20 already unsubscribed). Codes SendPulse adds later still deserialize; the `Explanation` text is
always present.

## Editing and cancelling

`UpdateAsync` edits a scheduled campaign; SendPulse expects every field of `UpdateCampaignRequest`,
not a partial patch. `CancelAsync` cancels a campaign that has not finished sending.
