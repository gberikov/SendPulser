using System.Net;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class ErrorHandlingTests
{
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
}
