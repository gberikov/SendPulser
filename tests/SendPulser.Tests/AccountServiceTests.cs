using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class SenderServiceTests
{
    [Fact]
    public async Task Lists_senders_with_their_smtp_flag()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(
            """[{"email":"a@x.com","name":"A","status":"Active","is_allowed_for_smtp":true},{"email":"b@x.com","name":"B","status":"Active","is_allowed_for_smtp":0}]""");

        var senders = await client.Senders.GetAllAsync();

        Assert.Equal(2, senders.Count);
        Assert.True(senders[0].IsAllowedForSmtp);
        Assert.False(senders[1].IsAllowedForSmtp);
        Assert.Equal("/senders", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Adds_deletes_and_activates_a_sender()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"result":true}""")
            .RespondWith("""{"result":true}""")
            .RespondWith("""{"result":true,"email":"a@x.com"}""")
            .RespondWith("""{"result":true,"email":"a@x.com"}""");

        await client.Senders.AddAsync("a@x.com", "A");
        Assert.Equal("""{"email":"a@x.com","name":"A"}""", handler.LastRequest.Body);

        await client.Senders.DeleteAsync("a@x.com");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("""{"email":"a@x.com"}""", handler.LastRequest.Body);

        await client.Senders.RequestActivationCodeAsync("a@x.com");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("/senders/a%40x.com/code", handler.LastRequest.Path);

        await client.Senders.ActivateAsync("a@x.com", "50405");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("""{"code":"50405"}""", handler.LastRequest.Body);
    }
}

public class BlacklistServiceTests
{
    [Fact]
    public async Task Sends_the_addresses_comma_joined_and_Base64_encoded()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""").RespondWith("""{"result":true}""");

        await client.Blacklist.AddAsync(["user1@mailserver.com", "user2@mailserver.com", "user3@mailserver.com"], "comment");

        Assert.Equal("/blacklist", handler.LastRequest.Path);
        Assert.Equal(
            """{"emails":"dXNlcjFAbWFpbHNlcnZlci5jb20sdXNlcjJAbWFpbHNlcnZlci5jb20sdXNlcjNAbWFpbHNlcnZlci5jb20=","comment":"comment"}""",
            handler.LastRequest.Body);

        await client.Blacklist.RemoveAsync(["user1@mailserver.com"]);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.DoesNotContain("comment", handler.LastRequest.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reads_the_blacklist_and_refuses_an_empty_batch()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""["a@x.com","b@x.com"]""");

        Assert.Equal(["a@x.com", "b@x.com"], await client.Blacklist.GetAllAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => client.Blacklist.AddAsync([]));
    }
}

public class TagServiceTests
{
    private const string Queued =
        """{"code":"request_sended_to_queue","description":"Request sended to queue","failure":false,"http_code":200,"queue_id":"9621c9bf","success":true}""";

    [Fact]
    public async Task Unwraps_the_tags_envelope()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"tags":[{"color":"#f0f4f6","id":2506,"name":"test1"}],"user_id":7043663,"version":"2.7.3"}""");

        var tags = await client.Tags.GetAllAsync();

        Assert.Single(tags);
        Assert.Equal(2506, tags[0].Id);
        Assert.Equal("#f0f4f6", tags[0].Color);
    }

    [Fact]
    public async Task Returns_the_queue_acknowledgement_of_every_write()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(Queued).RespondWith(Queued).RespondWith(Queued).RespondWith(Queued);

        var created = await client.Tags.CreateAsync("test3", "#f0f4f6");
        Assert.Equal("9621c9bf", created.QueueId);
        Assert.Equal("""{"name":"test3","color":"#f0f4f6"}""", handler.LastRequest.Body);

        await client.Tags.UpdateAsync(2505, "newName", "#f0f4f6");
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/tags/2505", handler.LastRequest.Path);

        await client.Tags.DeleteAsync(2505);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);

        await client.Tags.AssignToEmailAsync("test@test.com", [11111, 22222]);
        Assert.Equal("/tags/pin/email", handler.LastRequest.Path);
        Assert.Equal("""{"email":"test@test.com","tags":[11111,22222]}""", handler.LastRequest.Body);
    }

    [Fact]
    public async Task Fails_when_the_request_was_not_queued()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"success":false,"failure":true,"description":"tag exists"}""");

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(
            () => client.Tags.UnassignFromPhoneAsync("123", [1]));

        Assert.Contains("tag exists", exception.Message, StringComparison.Ordinal);
    }
}

public class BalanceServiceTests
{
    [Fact]
    public async Task Reads_the_balance_in_a_currency()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"currency":"USD","balance_currency":0.02}""");

        var balance = await client.Balance.GetAsync("usd");

        Assert.Equal(0.02m, balance.Amount);
        Assert.Equal("/balance/USD", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reads_the_plan_details_of_every_service()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("balance-detail.json"));

        var details = await client.Balance.GetDetailsAsync();

        Assert.Equal(9.36m, details.Balance!.Main);
        Assert.Equal(9914, details.Email!.EmailsLeft);
        Assert.True(details.Smtp!.AutoRenew);
        Assert.Equal(new DateTime(2018, 11, 30), details.Push!.EndsAt);
        Assert.Equal("/user/balance/detail", handler.LastRequest.Path);
    }
}

public class EmailAddressServiceTests
{
    [Fact]
    public async Task Reads_the_memberships_of_an_address_with_typed_variables()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(
            """[{"book_id":154441,"email":"t@g.com","status":0,"status_explain":"New","variables":[]},{"book_id":422325,"email":"t@g.com","status":1,"status_explain":"Active","variables":[{"name":"Name","type":"string","value":"Alona"}]}]""");

        var memberships = await client.EmailAddresses.GetAsync("t@g.com");

        Assert.Equal(2, memberships.Count);
        Assert.Empty(memberships[0].Variables);
        Assert.Equal("Alona", memberships[1].Variables[0].Value);
        Assert.Equal("/emails/t%40g.com", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reads_statistics_whichever_key_names_the_mailing_list()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"statistic":{"sent":9,"open":2,"link":0},"blacklist":false,"addressbooks":[{"id":154441,"address_book_name":"Mailing list 1"}]}""")
            .RespondWith("""{"a@y.com":{"sent":21,"open":11,"link":3,"adressbooks":[{"id":1375516,"name":"book1"}],"blacklist":true}}""");

        var single = await client.EmailAddresses.GetStatisticsAsync("a@y.com");
        Assert.Equal(9, single.Counters.Sent);
        Assert.Equal("Mailing list 1", single.AddressBooks[0].Name);

        var batch = await client.EmailAddresses.GetStatisticsAsync(["a@y.com"]);
        Assert.True(batch["a@y.com"].IsBlacklisted);
        Assert.Equal("book1", batch["a@y.com"].AddressBooks[0].Name);
        Assert.Equal("/emails/campaigns", handler.LastRequest.Path);
        Assert.Equal("""{"emails":["a@y.com"]}""", handler.LastRequest.Body);
    }

    [Fact]
    public async Task Looks_up_details_and_batches_and_deletes_everywhere()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""[{"list_name":"test1","list_id":1375516,"add_date":"2017-11-21 11:45:41","source":"panel"}]""")
            .RespondWith("""{"t1@g.com":[{"book_id":154441,"status":0,"variables":[]}]}""")
            .RespondWith("""{"result":true}""");

        var details = await client.EmailAddresses.GetDetailsAsync("t1@g.com");
        Assert.Equal("panel", details[0].Source);
        Assert.Equal(new DateTime(2017, 11, 21, 11, 45, 41), details[0].AddedAt);

        var many = await client.EmailAddresses.GetManyAsync(["t1@g.com"]);
        Assert.Equal(154441, many["t1@g.com"][0].AddressBookId);

        await client.EmailAddresses.DeleteFromAllListsAsync("t1@g.com");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/emails/t1%40g.com", handler.LastRequest.Path);
    }
}

public class AddressBookExtrasTests
{
    [Fact]
    public async Task Reads_a_single_contact_with_typed_variables()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(
            """{"email":"t@g.com","abook_id":"422325","phone":"","status":1,"status_explain":"Active","variables":[{"name":"Name","type":"string","value":"Test"}]}""");

        var contact = await client.AddressBooks.GetContactAsync(422325, "t@g.com");

        Assert.Equal(422325, contact.AddressBookId);
        Assert.Equal("Test", contact.Variables[0].Value);
        Assert.Equal("/addressbooks/422325/emails/t%40g.com", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Filters_contacts_by_activity_and_by_variable()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("[]").RespondWith("""[{"email":"a@y.com","status":0,"status_explain":"New"}]""");

        await client.AddressBooks.GetContactsAsync(1, limit: 10, active: true);
        Assert.Equal("?limit=10&active=1", handler.LastRequest.Query);

        var found = await client.AddressBooks.FindContactsByVariableAsync(1, "city", "Kyiv");
        Assert.Single(found);
        Assert.Equal("/addressbooks/1/variables/city/Kyiv", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Updates_variables_phone_and_subscription_state()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""").RespondWith("""{"result":true}""").RespondWith("""{"result":true}""");

        await client.AddressBooks.UpdateVariablesAsync(1, "m@e.com", [new("name", "John"), new("date", "2019-02-01")]);
        Assert.Equal("/addressbooks/1/emails/variable", handler.LastRequest.Path);
        Assert.Equal("""{"email":"m@e.com","variables":[{"name":"name","value":"John"},{"name":"date","value":"2019-02-01"}]}""", handler.LastRequest.Body);

        await client.AddressBooks.SetPhoneAsync(1, "m@e.com", "1234567890");
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/addressbooks/1/phone", handler.LastRequest.Path);

        await client.AddressBooks.UnsubscribeContactsAsync(1, ["m@e.com"]);
        Assert.Equal("/addressbooks/1/emails/unsubscribe", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reads_the_cost_estimate_and_the_campaigns_of_a_list()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"cur":"USD","sent_emails_qty":16,"overdraftAllEmailsPrice":0,"addressesDeltaFromBalance":0,"addressesDeltaFromTariff":16,"max_emails_per_task":500,"result":true}""")
            .RespondWith("""[{"task_id":9147593,"task_name":"Test","task_status":3}]""");

        var cost = await client.AddressBooks.GetCampaignCostAsync(1);
        Assert.True(cost.IsAffordable);
        Assert.Equal(500, cost.MaxEmailsPerCampaign);

        var campaigns = await client.AddressBooks.GetCampaignsAsync(1, limit: 5);
        Assert.Equal(9147593, campaigns[0].Id);
        Assert.Equal("/addressbooks/1/campaigns", handler.LastRequest.Path);
        Assert.Equal("?limit=5", handler.LastRequest.Query);
    }
}

public class CampaignExtrasTests
{
    [Fact]
    public async Task Passes_every_list_filter_through()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("[]");

        await client.Campaigns.GetAllAsync(limit: 100, offset: 10, order: "desc", statuses: [3, 26], scheduled: true);

        Assert.Equal("?limit=100&offset=10&order=desc&planed=1&status%5B%5D=3&status%5B%5D=26", handler.LastRequest.Query);
    }

    [Fact]
    public async Task Reads_country_referral_and_recipient_statistics()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"US":23,"UA":34567}""")
            .RespondWith("""[{"link":"http://first_link.com","count":123454}]""")
            .RespondWith("""{"sent_date":"2021-03-29 07:46:58","global_status":1,"global_status_explain":"Sent","detail_status":3,"detail_status_explain":"Opened"}""");

        var countries = await client.Campaigns.GetCountryStatisticsAsync(7);
        Assert.Equal(34567, countries["UA"]);
        Assert.Equal("/campaigns/7/countries", handler.LastRequest.Path);

        var referrals = await client.Campaigns.GetReferralStatisticsAsync(7);
        Assert.Equal(123454, referrals[0].Count);

        var recipient = await client.Campaigns.GetRecipientAsync(7, "a@b.com");
        Assert.Equal(3, recipient.DetailStatus);
        Assert.Equal("/campaigns/7/email/a%40b.com", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reads_the_price_fields_whether_numbers_or_strings()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("campaign.json"));

        var campaign = await client.Campaigns.GetAsync(14973974);

        Assert.Equal(0.05m, campaign.Price);
        Assert.Equal(0m, campaign.OverdraftPrice);
        Assert.True(campaign.Tracking!.TracksOpens);
    }
}

public class SmtpExtrasTests
{
    [Fact]
    public async Task Reads_tracking_of_a_message()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("smtp-email.json"));

        var email = await client.Smtp.GetEmailAsync("pzkic9-0afezp-fc");

        Assert.Equal(1, email.Tracking!.Opens);
        Assert.Equal("http://some-url.com", email.Tracking.Links[0].Url);
        Assert.Equal("Thunderbird 17.0.8", email.Tracking.Clients[0].Browser);
        Assert.Equal(new DateTime(2013, 9, 30, 11, 27, 49), email.Tracking.Clients[0].ActionDate);
    }

    [Fact]
    public async Task Reads_bounces_and_their_total()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"total":1,"emails":[{"email_to":"x@y.z","sender":"m@e.com","send_date":"2025-07-18 05:01:07","subject":"g","smtp_answer_code":550,"smtp_answer_subcode":"5.1.3","smtp_answer_data":"No MX"}],"request_limit":100,"found":1}""")
            .RespondWith("""{"total":3}""");

        var page = await client.Smtp.GetBouncesAsync(new DateOnly(2025, 7, 18), limit: 10, offset: 20);
        Assert.Equal("x@y.z", page.Bounces[0].Recipient);
        Assert.Equal("5.1.3", page.Bounces[0].SmtpAnswerSubcode);
        Assert.Equal("?date=2025-07-18&limit=10&offset=20", handler.LastRequest.Query);

        Assert.Equal(3, await client.Smtp.GetBounceCountAsync());
        Assert.Equal("/smtp/bounces/day/total", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Treats_result_false_of_the_unsubscribe_search_as_data()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":false}""").RespondWith("""{"result":true}""");

        Assert.False(await client.Smtp.IsUnsubscribedAsync("a@b.com"));
        Assert.Equal("?email=a%40b.com", handler.LastRequest.Query);
        Assert.True(await client.Smtp.IsUnsubscribedAsync("a@b.com"));
    }

    [Fact]
    public async Task Resubscribes_reads_ids_and_infos()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"result":true,"id":"pzkic9-0afezp-fc"}""")
            .RespondWith("""["127.0.0.1"]""")
            .RespondWith("""[{"id":"a"},{"id":123}]""");

        var sent = await client.Smtp.ResubscribeAsync("t@t.com", "s@t.com", "en");
        Assert.Equal("pzkic9-0afezp-fc", sent.Id);
        Assert.Equal("""{"email":"t@t.com","sender":"s@t.com","lang":"en"}""", handler.LastRequest.Body);

        Assert.Equal(["127.0.0.1"], await client.Smtp.GetIpAddressesAsync());

        var infos = await client.Smtp.GetEmailsAsync(["a", "123"]);
        Assert.Equal("123", infos[1].Id);
        Assert.Equal("/smtp/emails/info", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Unwraps_the_double_data_envelope_of_sender_domains()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"data":{"result":true,"data":[{"id":37857,"user_id":1,"service_type":3,"service_value":"example.com","status":0,"is_default":true,"ssl_type":0,"checks":{"check_dkim":false,"check_spf":true,"all_checks":false,"spf_txt_needed":"v=spf1"}}]}}""")
            .RespondWith("""{"data":{"result":true,"error":null}}""")
            .RespondWith("""{"data":{"result":false,"error":"domain exists"}}""");

        var domains = await client.Smtp.GetSenderDomainsAsync();
        Assert.Equal("example.com", domains[0].Domain);
        Assert.True(domains[0].Checks!.Spf);

        await client.Smtp.AddSenderDomainAsync("example.com");
        Assert.Equal("/v2/email-service/smtp/sender_domains/example.com", handler.LastRequest.Path);

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() => client.Smtp.AddSenderDomainAsync("example.com"));
        Assert.Contains("domain exists", exception.Message, StringComparison.Ordinal);
    }
}

public class WebhookServiceExtrasTests
{
    [Fact]
    public async Task Fails_when_an_update_or_delete_is_rejected_inside_the_envelope()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler
            .RespondWith("""{"success":false,"data":[false]}""")
            .RespondWith("""{"data":{"result":false}}""")
            .RespondWith("""{"success":true,"data":[true]}""");

        await Assert.ThrowsAsync<SendPulserApiException>(() => client.Webhooks.UpdateAsync(1, "https://x.y/z"));
        await Assert.ThrowsAsync<SendPulserApiException>(() => client.Webhooks.DeleteAsync(1));
        await client.Webhooks.DeleteAsync(1);
    }

    [Fact]
    public async Task Reads_the_success_envelope_of_the_documented_v2_shape()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"success":true,"data":{"id":162242,"user_id":7043663,"url":"https://site.com/callback","action":"unsubscribe"}}""");

        var webhook = await client.Webhooks.GetAsync(162242);

        Assert.Equal("unsubscribe", webhook.Action);
    }
}

public class OptionsTests
{
    [Fact]
    public void Refuses_a_plain_http_base_address_that_is_not_local()
    {
        var options = new SendPulserOptions
        {
            ClientId = "id",
            ClientSecret = "secret",
            BaseAddress = new Uri("http://api.sendpulse.com/"),
        };

        var exception = Assert.Throws<ArgumentException>(options.Validate);
        Assert.Contains("https", exception.Message, StringComparison.Ordinal);

        options.BaseAddress = new Uri("http://localhost:5000/");
        options.Validate();
    }

    [Fact]
    public void Requires_a_base_address_on_a_supplied_HttpClient()
    {
        using var httpClient = new HttpClient();

        Assert.Throws<ArgumentException>(() => new SendPulserClient(httpClient));
    }

    [Fact]
    public async Task Wraps_a_connection_failure_in_a_transport_exception()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.Respond(_ => throw new HttpRequestException("connection refused"));

        var exception = await Assert.ThrowsAsync<SendPulserTransportException>(() => client.AddressBooks.GetAllAsync());

        Assert.IsType<HttpRequestException>(exception.InnerException);
    }
}
