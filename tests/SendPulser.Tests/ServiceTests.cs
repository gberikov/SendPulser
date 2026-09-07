using System.Text.Json;
using SendPulser.Campaigns;
using SendPulser.Smtp;
using SendPulser.Templates;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class TemplateServiceTests
{
    [Fact]
    public async Task Lists_templates_filtered_by_owner()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("templates.json"));

        var templates = await client.Templates.GetAllAsync("me");

        Assert.Equal(2, templates.Count);
        Assert.Equal("/templates", handler.LastRequest.Path);
        Assert.Equal("?owner=me", handler.LastRequest.Query);
    }

    [Fact]
    public async Task Reads_a_single_template()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("template.json"));

        var template = await client.Templates.GetAsync("f3266876955c9d21e214deed49b97446");

        Assert.Equal(1153018, template.RealId);
        Assert.Equal("/template/f3266876955c9d21e214deed49b97446", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Creates_a_template_and_returns_the_new_id()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true,"real_id":1042220}""");

        var result = await client.Templates.CreateAsync(new CreateTemplateRequest
        {
            Name = "My Template",
            Body = "<p>hi</p>",
            Language = "en",
        });

        Assert.Equal(1042220, result.RealId);
        Assert.Equal("/template", handler.LastRequest.Path);
        Assert.Equal("PHA+aGk8L3A+", JsonBody.Property(handler.LastRequest.Body, "body"));
    }

    [Fact]
    public async Task Edits_a_template_through_the_edit_path()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.Templates.UpdateAsync("1042220", new UpdateTemplateRequest { Body = "<p>hi</p>" });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/template/edit/1042220", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Rejects_an_empty_template_id()
    {
        var (client, _) = TestClient.Create();
        using var scope = client;

        await Assert.ThrowsAsync<ArgumentException>(() => client.Templates.GetAsync("  "));
    }
}

public class CampaignServiceTests
{
    [Fact]
    public async Task Lists_campaigns_with_their_counters()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("campaigns.json"));

        var campaigns = await client.Campaigns.GetAllAsync(limit: 100);

        Assert.Single(campaigns);
        Assert.Equal(2, campaigns[0].Statistics!.Delivered);
        Assert.Equal("e.b@sendpulse.com", campaigns[0].Message!.SenderEmail);
    }

    [Fact]
    public async Task Reads_a_campaign_with_its_per_status_statistics()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("campaign.json"));

        var campaign = await client.Campaigns.GetAsync(14973974);

        Assert.Equal(2, campaign.Statistics!.General.Count);
        Assert.Equal("Opened", campaign.Statistics.General[1].Explanation);
        Assert.Equal(3, campaign.Statistics.Clicks[0].Count);
        Assert.Equal("/campaigns/14973974", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Creates_a_campaign()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"id":245587,"status":13,"count":1,"tariff_email_qty":1}""");

        var result = await client.Campaigns.CreateAsync(new CreateCampaignRequest
        {
            SenderName = "Sender",
            SenderEmail = "sender@example.com",
            Subject = "Hello",
            ListIds = [756589],
            TemplateId = "775667",
            AmpBody = "<p>AMP</p>",
            BinaryAttachments = new() { ["invoice.pdf"] = [1, 2, 3] },
            Statistics = new() { Clicks = true, Opens = false, UtmCampaign = "launch" },
        });

        Assert.Equal(245587, result.Id);
        Assert.Equal("/campaigns", handler.LastRequest.Path);
        Assert.Contains("\"list_id\":756589", handler.LastRequest.Body, StringComparison.Ordinal);
        Assert.Contains("\"body_amp\":\"PHA+QU1QPC9wPg==\"", handler.LastRequest.Body, StringComparison.Ordinal);
        Assert.Contains("\"attachments_binary\":{\"invoice.pdf\":\"AQID\"}", handler.LastRequest.Body, StringComparison.Ordinal);
        using var body = JsonDocument.Parse(handler.LastRequest.Body!);
        var stats = body.RootElement.GetProperty("stats");
        Assert.True(stats.GetProperty("clicks").GetBoolean());
        Assert.False(stats.GetProperty("opens").GetBoolean());
        Assert.Equal("launch", stats.GetProperty("utm_campaign").GetString());
        Assert.False(body.RootElement.TryGetProperty("utm_campaign", out var unused));
    }

    [Fact]
    public async Task Edits_a_scheduled_campaign_with_a_patch()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true,"id":470791}""");

        await client.Campaigns.UpdateAsync(470791, new UpdateCampaignRequest
        {
            Name = "renamed",
            SenderName = "Sender",
            SenderEmail = "sender@example.com",
            Subject = "Hello",
            SendDate = new DateTime(2026, 7, 6, 11, 45, 0, DateTimeKind.Utc),
        });

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Contains("\"send_date\":\"2026-07-06 11:45:00\"", handler.LastRequest.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Cancels_a_campaign()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.Campaigns.CancelAsync(245587);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/campaigns/245587", handler.LastRequest.Path);
    }
}

public class SmtpServiceTests
{
    [Fact]
    public async Task Wraps_a_message_in_the_email_envelope()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true,"id":"pzkic9-0afezp-fc"}""");

        var result = await client.Smtp.SendAsync(new SendEmailRequest
        {
            Subject = "Example subject",
            Html = "<p>Example</p>",
            From = new EmailAddress("sender@example.com", "Example name"),
            To = [new EmailAddress("recipient@example.com")],
        });

        Assert.Equal("pzkic9-0afezp-fc", result.Id);
        Assert.Equal("/smtp/emails", handler.LastRequest.Path);
        Assert.StartsWith("{\"email\":{", handler.LastRequest.Body, StringComparison.Ordinal);
        Assert.Equal("PHA+RXhhbXBsZTwvcD4=", JsonBody.Property(handler.LastRequest.Body, "email", "html"));
    }

    [Fact]
    public async Task Filters_the_message_list_by_date_and_participants()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("smtp-emails.json"));

        var emails = await client.Smtp.GetEmailsAsync(
            limit: 10,
            fromDate: new DateOnly(2026, 1, 1),
            toDate: new DateOnly(2026, 1, 31),
            recipient: "a@b.com");

        Assert.Single(emails);
        Assert.Equal("?limit=10&from=2026-01-01&to=2026-01-31&recipient=a%40b.com", handler.LastRequest.Query);
    }

    [Fact]
    public async Task Can_omit_country_data_from_processed_messages()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("[]");

        await client.Smtp.GetEmailsAsync(includeCountry: false, limit: 10);

        Assert.Equal("?limit=10&country=off", handler.LastRequest.Query);
    }

    [Fact]
    public async Task Reads_the_total_number_of_messages()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"total":25408}""");

        Assert.Equal(25408, await client.Smtp.GetTotalCountAsync());
    }

    [Fact]
    public async Task Unsubscribes_addresses_with_their_comments()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.Smtp.UnsubscribeAsync([new UnsubscribeRequest("bad@example.com", "bounced")]);

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            """{"emails":[{"email":"bad@example.com","comment":"bounced"}]}""",
            handler.LastRequest.Body);
    }

    [Fact]
    public async Task Removes_addresses_from_the_unsubscribe_list()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.Smtp.RemoveFromUnsubscribeListAsync(["bad@example.com"]);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/smtp/unsubscribe", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reads_unsubscribed_contacts_of_one_day()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(
            """[{"email":"a@b.com","unsubscribe_by_link":1,"unsubscribe_by_user":0,"spam_complaint":1,"date":"2018-11-24 19:19:01"}]""");

        var contacts = await client.Smtp.GetUnsubscribedAsync(new DateOnly(2018, 11, 24));

        Assert.True(contacts[0].SpamComplaint);
        Assert.Equal("?date=2018-11-24", handler.LastRequest.Query);
    }

    [Fact]
    public async Task Reads_the_sender_addresses()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""["sender@domain.com"]""");

        var senders = await client.Smtp.GetSendersAsync();

        Assert.Equal(["sender@domain.com"], senders);
    }
}

public class WebhookServiceTests
{
    [Fact]
    public async Task Unwraps_the_nested_data_envelope_of_the_v2_endpoints()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("webhooks.json"));

        var webhooks = await client.Webhooks.GetAllAsync();

        Assert.Equal(2, webhooks.Count);
        Assert.Equal("delivered", webhooks[0].Action);
        Assert.Equal("/v2/email-service/webhook", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Accepts_a_single_registration_returned_as_a_bare_object()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"id":1,"user_id":2,"url":"https://example.com","action":"open"}""");

        var webhook = await client.Webhooks.GetAsync(1);

        Assert.Equal("open", webhook.Action);
    }

    [Fact]
    public async Task Subscribes_a_url_to_several_events()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("webhooks.json"));

        var created = await client.Webhooks.CreateAsync("https://example.com/hooks/sp", ["delivered", "open"]);

        Assert.Equal(2, created.Count);
        Assert.Equal("application/x-www-form-urlencoded", handler.LastRequest.ContentType);
        Assert.Equal(
            "url=https%3A%2F%2Fexample.com%2Fhooks%2Fsp&actions%5B%5D=delivered&actions%5B%5D=open",
            handler.LastRequest.Body);
    }

    [Fact]
    public async Task Deletes_a_registration()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"data":{"result":true}}""");

        await client.Webhooks.DeleteAsync(37857);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/v2/email-service/webhook/37857", handler.LastRequest.Path);
    }
}
