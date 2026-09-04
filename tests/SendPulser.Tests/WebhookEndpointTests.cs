using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendPulser.Tests.Infrastructure;
using SendPulser.Webhooks;

namespace SendPulser.Tests;

public class WebhookEndpointTests
{
    private const string Secret = "s3cr3t";

    [Fact]
    public async Task Parses_a_batch_of_bulk_email_events_into_typed_events()
    {
        List<EmailWebhookEvent> received = [];
        using var host = await StartEmailHostAsync((events, _) =>
        {
            received.AddRange(events);
            return Task.CompletedTask;
        });

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, $"/hooks/{Secret}", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(9, received.Count);

        var opened = Assert.IsType<EmailOpenedEvent>(received[0]);
        Assert.Equal("Firefox", opened.BrowserName);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1496827941), opened.Timestamp);
        Assert.Equal(3668141, opened.TaskId);

        var clicked = Assert.IsType<EmailClickedEvent>(received[1]);
        Assert.Equal("http://google.com", clicked.LinkUrl);

        var unsubscribed = Assert.IsType<EmailUnsubscribedEvent>(received[2]);
        Assert.True(unsubscribed.FromAll);
        Assert.Equal(490686, unsubscribed.BookId);

        Assert.IsType<EmailDeliveredEvent>(received[3]);
    }

    [Fact]
    public async Task Surfaces_an_event_it_does_not_know_instead_of_failing()
    {
        List<EmailWebhookEvent> received = [];
        using var host = await StartEmailHostAsync((events, _) =>
        {
            received.AddRange(events);
            return Task.CompletedTask;
        });

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, $"/hooks/{Secret}", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var unknown = Assert.IsType<UnknownEmailEvent>(received[4]);
        Assert.Equal("hard_bounce_unknown_to_the_sdk", unknown.Event);
        Assert.Equal("test@e.cn.ua", unknown.Email);
        Assert.Equal("value", unknown.Raw.GetProperty("some_new_field").GetString());
    }

    [Fact]
    public async Task Parses_smtp_events_with_their_own_field_names()
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
        using var response = await PostFixtureAsync(client, "/hooks/smtp", "smtp-webhook-batch.json");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var delivered = Assert.IsType<SmtpDeliveredEvent>(received[0]);
        Assert.Equal("1149317311", delivered.MessageId);
        Assert.Equal(250, delivered.ResponseCode);
        Assert.Equal("doe.john@sendpulse.com", delivered.Recipient);

        var opened = Assert.IsType<SmtpOpenedEvent>(received[1]);
        Assert.Equal("1149317311", opened.MessageId);
    }

    [Fact]
    public async Task Hides_the_endpoint_when_the_secret_does_not_match()
    {
        var called = false;
        using var host = await StartEmailHostAsync((_, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, "/hooks/wrong", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(called);
    }

    [Fact]
    public async Task Answers_500_when_the_handler_throws_so_the_failure_is_visible()
    {
        using var host = await StartEmailHostAsync((_, _) => throw new InvalidOperationException("database is down"));

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, $"/hooks/{Secret}", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_body_that_is_not_json()
    {
        using var host = await StartEmailHostAsync((_, _) => Task.CompletedTask);

        using var client = host.GetTestClient();
        using var content = new StringContent("not json at all", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(new Uri($"/hooks/{Secret}", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Accepts_a_single_event_posted_as_a_bare_object()
    {
        List<EmailWebhookEvent> received = [];
        using var host = await StartEmailHostAsync((events, _) =>
        {
            received.AddRange(events);
            return Task.CompletedTask;
        });

        using var client = host.GetTestClient();
        using var content = new StringContent(
            """{"event":"delivered","timestamp":1632316421,"task_id":9333331,"email":"a@b.com"}""",
            Encoding.UTF8,
            "application/json");
        using var response = await client.PostAsync(new Uri($"/hooks/{Secret}", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Single(received);
    }

    [Fact]
    public async Task Accepts_the_secret_from_the_query_string_when_the_route_has_no_segment_for_it()
    {
        List<EmailWebhookEvent> received = [];
        using var host = await StartHostAsync(endpoints => endpoints.MapSendPulserEmailWebhook(
            "/hooks/email",
            (events, _) =>
            {
                received.AddRange(events);
                return Task.CompletedTask;
            },
            options => options.Secret = Secret));

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, $"/hooks/email?secret={Secret}", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEmpty(received);
    }

    [Fact]
    public async Task Refuses_a_caller_outside_the_address_allowlist()
    {
        using var host = await StartHostAsync(endpoints => endpoints.MapSendPulserEmailWebhook(
            "/hooks/email",
            (_, _) => Task.CompletedTask,
            options => options.AllowedAddresses.Add(IPAddress.Parse("203.0.113.7"))));

        using var client = host.GetTestClient();
        using var response = await PostFixtureAsync(client, "/hooks/email", "email-webhook-batch.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static Task<IHost> StartEmailHostAsync(
        Func<IReadOnlyList<EmailWebhookEvent>, CancellationToken, Task> handler) =>
        StartHostAsync(endpoints => endpoints.MapSendPulserEmailWebhook(
            "/hooks/{secret}",
            handler,
            options => options.Secret = Secret));

    private static async Task<IHost> StartHostAsync(Action<IEndpointRouteBuilder> map)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(builder => builder
                .UseTestServer()
                .ConfigureServices(services => services.AddRouting())
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => map(endpoints));
                }))
            .StartAsync();

        return host;
    }

    private static Task<HttpResponseMessage> PostFixtureAsync(HttpClient client, string path, string fixture)
    {
        var content = new StringContent(TestClient.Fixture(fixture), Encoding.UTF8, "application/json");
        return client.PostAsync(new Uri(path, UriKind.Relative), content);
    }
}
