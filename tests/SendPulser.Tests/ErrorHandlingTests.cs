using System.Net;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class ErrorHandlingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancels_response_body_reads_on_timeout_or_caller_cancellation(bool cancelFromCaller)
    {
        using var cancellation = new CancellationTokenSource();
        var handler = new FakeHttpMessageHandler().Respond(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new DelayedResponseContent(),
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = TestClient.BaseAddress,
            Timeout = cancelFromCaller ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(100),
        };
        using var client = new SendPulserClient(httpClient);
        if (cancelFromCaller)
        {
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(100));
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.AddressBooks.GetAllAsync(cancellationToken: cancellation.Token));
    }

    [Fact]
    public async Task Redacts_encoded_email_segments_in_api_errors()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("{}", HttpStatusCode.NotFound);

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() =>
            client.EmailAddresses.GetAsync("alice+tag@example.com"));

        Assert.Contains("emails/{email}", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("alice", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{\"result\":false}")]
    [InlineData("{\"result\":false,\"message\":\"Rejected\"}")]
    [InlineData("{}")]
    public async Task Redacts_email_segments_when_a_write_is_not_accepted(string body)
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(body);

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() =>
            client.EmailAddresses.DeleteFromAllListsAsync("alice+tag@example.com"));

        Assert.Contains("emails/{email}", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("alice", exception.Message, StringComparison.Ordinal);
        Assert.Equal(body, exception.ResponseBody);
    }

    [Fact]
    public async Task Redacts_email_segments_when_a_read_returns_null()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("null");

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() =>
            client.EmailAddresses.GetAsync("alice+tag@example.com"));

        Assert.Contains("emails/{email}", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("alice", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Redacts_email_segments_in_transport_errors()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        var failure = new HttpRequestException("Connection failed");
        handler.Respond(_ => throw failure);

        var exception = await Assert.ThrowsAsync<SendPulserTransportException>(() =>
            client.EmailAddresses.GetAsync("alice+tag@example.com"));

        Assert.Contains("emails/{email}", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("alice", exception.Message, StringComparison.Ordinal);
        Assert.Same(failure, exception.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Omits_query_strings_from_api_and_transport_errors(bool transportFailure)
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        if (transportFailure)
        {
            handler.Respond(_ => throw new HttpRequestException("Connection failed"));
        }
        else
        {
            handler.RespondWith("null");
        }

        var exception = await Record.ExceptionAsync(() => client.Smtp.IsUnsubscribedAsync("alice+tag@example.com"));

        Assert.NotNull(exception);
        Assert.Contains("smtp/unsubscribe/search", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("?", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("alice", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Turns_an_http_failure_into_an_api_exception_carrying_the_status_and_the_body()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(
            """{"is_error":true,"http_code":404,"error_code":209,"message":"Book not found"}""",
            HttpStatusCode.NotFound);

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.GetAllAsync());

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal(209, exception.ErrorCode);
        Assert.Contains("Book not found", exception.Message, StringComparison.Ordinal);
        Assert.Contains("is_error", exception.ResponseBody!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fails_on_an_is_error_payload_returned_with_http_200()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;

        // SendPulse reports part of its failures without leaving the 2xx range.
        handler.RespondWith("""{"is_error":true,"error_code":301,"message":"Wrong parameters"}""");

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.GetAllAsync());

        Assert.Equal(301, exception.ErrorCode);
    }

    [Fact]
    public async Task Names_the_per_second_ceiling_when_SendPulse_reports_it()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(
            """{"is_error":true,"error_code":2020202020,"message":"More than 10 requests per second"}""",
            HttpStatusCode.TooManyRequests);

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.GetAllAsync());

        Assert.Equal(RateLimit.PerSecondErrorCode, exception.ErrorCode);
        Assert.Contains("10 requests per second", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Passes_through_a_body_that_is_not_json_in_the_failure_message()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("<html>502 Bad Gateway</html>", HttpStatusCode.BadGateway, "text/html");

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.GetAllAsync());

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Contains("502 Bad Gateway", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fails_when_a_write_is_answered_with_result_false()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":false}""");

        await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.DeleteAsync(1));
    }

    [Fact]
    public async Task Fails_when_a_typed_success_response_is_missing_required_fields()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("{}");

        await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.CreateAsync("book"));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"status\":13,\"count\":1}")]
    public async Task Fails_when_a_created_campaign_has_no_id(string body)
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(body);

        await Assert.ThrowsAsync<SendPulserApiException>(() => client.Campaigns.CreateAsync(new()));
    }

    [Theory]
    [InlineData("smtp-send")]
    [InlineData("smtp-resubscribe")]
    [InlineData("template")]
    public async Task Preserves_the_reason_code_and_body_when_a_typed_write_is_rejected(string operation)
    {
        const string body = """{"result":false,"error":"Sender is not verified","error_code":123}""";
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(body);

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() => operation switch
        {
            "smtp-send" => client.Smtp.SendAsync(new()),
            "smtp-resubscribe" => client.Smtp.ResubscribeAsync("recipient@example.com", "sender@example.com"),
            "template" => (Task)client.Templates.CreateAsync(new()),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        });

        Assert.Equal(HttpStatusCode.OK, exception.StatusCode);
        Assert.Equal(123, exception.ErrorCode);
        Assert.Equal(body, exception.ResponseBody);
        Assert.Contains("Sender is not verified", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Treats_a_false_subscription_lookup_as_data()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":false}""");

        Assert.False(await client.Smtp.IsUnsubscribedAsync("alice@example.com"));
    }

    [Fact]
    public async Task Fails_when_smtp_does_not_accept_an_email()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":false}""");

        await Assert.ThrowsAsync<SendPulserApiException>(() => client.Smtp.SendAsync(new()));
    }

    [Fact]
    public async Task Fails_when_a_false_result_is_nested_below_a_success_flag()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"success":true,"data":[false]}""");

        await Assert.ThrowsAsync<SendPulserApiException>(() => client.Webhooks.DeleteAsync(1));
    }

    [Fact]
    public async Task Fails_when_the_payload_does_not_match_the_expected_shape()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"unexpected":"object instead of an array"}""");

        await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.GetAllAsync());
    }

    [Fact]
    public void Requires_credentials()
    {
        Assert.Throws<ArgumentException>(() => new SendPulserClient(new SendPulserOptions()));
    }

    [Fact]
    public void Adds_the_trailing_slash_a_relative_path_needs()
    {
        var options = new SendPulserOptions { BaseAddress = new Uri("https://api.sendpulse.com") };

        Assert.Equal("https://api.sendpulse.com/", options.GetNormalizedBaseAddress().AbsoluteUri);
    }

    private sealed class DelayedResponseContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            await stream.WriteAsync("[]"u8.ToArray(), cancellationToken);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 2;
            return true;
        }
    }
}
