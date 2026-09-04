using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendPulser.Tests.Infrastructure;
using SendPulser.Webhooks;

namespace SendPulser.Tests;

/// <summary>
/// The typed shape of every documented webhook event, plus the fallbacks for shapes SendPulse changes.
/// </summary>
public class WebhookEventTests
{
    [Fact]
    public async Task Types_every_documented_bulk_email_event()
    {
        var received = await ReceiveEmailEventsAsync("email-webhook-batch.json");

        var subscriber = Assert.IsType<EmailNewSubscriberEvent>(received[5]);
        Assert.Equal("subscription form", subscriber.Source);
        Assert.Equal(490686, subscriber.BookId);
        Assert.Equal("John", subscriber.Variables!["name"]);
        Assert.Equal("42", subscriber.Variables["age"]);

        var deleted = Assert.IsType<EmailDeletedEvent>(received[6]);
        Assert.Equal(490686, deleted.BookId);

        var status = Assert.IsType<EmailCampaignStatusEvent>(received[7]);
        Assert.Equal("approve", status.Status);
        Assert.Equal("Approved and will be sent", status.StatusExplanation);
        Assert.Equal(3668138, status.TaskId);

        var bounce = Assert.IsType<EmailSoftBounceEvent>(received[8]);
        Assert.Equal(550, bounce.ResponseCode);
        Assert.Equal("5.1.0", bounce.ResponseSubcode);
        Assert.Equal("example@example.com", bounce.Email);
    }

    [Fact]
    public async Task Keeps_fields_it_has_no_property_for_on_a_known_event()
    {
        var received = await ReceiveEmailEventsAsync("email-webhook-batch.json");

        var subscriber = Assert.IsType<EmailNewSubscriberEvent>(received[5]);
        Assert.Equal("kept", subscriber.AdditionalProperties!["brand_new_field"].GetString());

        var delivered = Assert.IsType<EmailDeliveredEvent>(received[3]);
        Assert.Null(delivered.AdditionalProperties);
    }

    [Fact]
    public async Task Types_every_documented_smtp_event()
    {
        var received = await ReceiveSmtpEventsAsync("smtp-webhook-batch.json");

        Assert.Equal(5, received.Count);

        var undelivered = Assert.IsType<SmtpUndeliveredEvent>(received[2]);
        Assert.Equal(554, undelivered.ResponseCode);
        Assert.Equal("5.7.1", undelivered.ResponseSubcode);
        Assert.Equal("1149317311", undelivered.MessageId);

        var spam = Assert.IsType<SmtpSpamEvent>(received[3]);
        Assert.Equal("1145317311", spam.MessageId);

        var bounce = Assert.IsType<SmtpHardBounceEvent>(received[4]);
        Assert.Equal("example@example.com", bounce.Email);
        Assert.Equal(17076325, bounce.TaskId);
        Assert.Equal("User unknown in local recipient table", bounce.Response);
    }

    [Fact]
    public async Task Hands_the_request_context_to_the_handler_when_asked()
    {
        IServiceProvider? seen = null;
        using var host = await StartHostAsync(endpoints => endpoints.MapSendPulserEmailWebhook(
            "/hooks/email",
            (_, context, _) =>
            {
                seen = context.RequestServices;
                return Task.CompletedTask;
            }));

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, "/hooks/email", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(seen);
    }

    [Fact]
    public async Task Refuses_a_body_larger_than_the_configured_limit()
    {
        using var host = await StartHostAsync(endpoints => endpoints.MapSendPulserEmailWebhook(
            "/hooks/email",
            (_, _) => Task.CompletedTask,
            options => options.MaxRequestBodySize = 64));

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, "/hooks/email", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    private static async Task<List<EmailWebhookEvent>> ReceiveEmailEventsAsync(string fixture)
    {
        List<EmailWebhookEvent> received = [];
        using var host = await StartHostAsync(endpoints => endpoints.MapSendPulserEmailWebhook(
            "/hooks/email",
            (events, _) =>
            {
                received.AddRange(events);
                return Task.CompletedTask;
            }));

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, "/hooks/email", fixture);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        return received;
    }

    private static async Task<List<SmtpWebhookEvent>> ReceiveSmtpEventsAsync(string fixture)
    {
        List<SmtpWebhookEvent> received = [];
        using var host = await StartHostAsync(endpoints => endpoints.MapSendPulserSmtpWebhook(
            "/hooks/smtp",
            (events, _) =>
            {
                received.AddRange(events);
                return Task.CompletedTask;
            }));

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, "/hooks/smtp", fixture);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        return received;
    }

    private static async Task<IHost> StartHostAsync(Action<IEndpointRouteBuilder> map) =>
        await new HostBuilder()
            .ConfigureWebHost(builder => builder
                .UseTestServer()
                .ConfigureServices(services => services.AddRouting())
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => map(endpoints));
                }))
            .StartAsync();

    private static Task<HttpResponseMessage> PostFixtureAsync(HttpClient client, string path, string fixture)
    {
        var content = new StringContent(TestClient.Fixture(fixture), System.Text.Encoding.UTF8, "application/json");
        return client.PostAsync(new Uri(path, UriKind.Relative), content);
    }
}
