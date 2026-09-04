# Templates

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

`client.Templates` wraps the template endpoints of the bulk email service.

| Method | SendPulse endpoint |
| --- | --- |
| `GetAllAsync(owner)` | `GET /templates` |
| `GetAsync(id)` | `GET /template/{id}` |
| `GetBySlugAsync(slug)` | `GET /template/slug/{slug}` |
| `CreateAsync(request)` | `POST /template` |
| `UpdateAsync(id, request)` | `POST /template/edit/{id}` |

```csharp
var mine = await client.Templates.GetAllAsync(owner: "me");        // "sendpulse" or null for the system ones

var created = await client.Templates.CreateAsync(new CreateTemplateRequest
{
    Name = "Welcome",
    Body = "<p>Hello {{name}}</p>",     // plain HTML, Base64 encoded on the wire
    Language = "en",
});

await client.Templates.UpdateAsync(created.RealId.ToString(), new UpdateTemplateRequest
{
    Body = "<p>Hi {{name}}</p>",
    Language = "en",                    // must match the language used at creation
});
```

Every template has two IDs: the string `Id` and the numeric `RealId`. Either is accepted wherever a
template is referenced, including `SendEmailRequest.Template` and `CreateCampaignRequest.TemplateId`.

The listing returns `Template` with its category, tags, owner and preview URL. SendPulse returns the
`tags` field as an object when tags exist and as an empty array when they do not, and `category_info`
as an empty array when the template has no category; both are normalised, `Tags` is always a list and
`CategoryInfo` is `null` when absent.
