using System.Net;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class TokenProviderTests
{
    private const string TokenJson = """{"access_token":"first","token_type":"Bearer","expires_in":3600}""";

    private static SendPulserOptions Options => new()
    {
        ClientId = "id",
        ClientSecret = "secret",
    };

    [Fact]
    public async Task Requests_a_token_once_and_reuses_it()
    {
        var handler = new FakeHttpMessageHandler().RespondWith(TokenJson);
        using var httpClient = new HttpClient(handler) { BaseAddress = TestClient.BaseAddress };
        using var provider = new SendPulserTokenProvider(httpClient, Options);

        Assert.Equal("first", await provider.GetTokenAsync());
        Assert.Equal("first", await provider.GetTokenAsync());

        Assert.Single(handler.Requests);
        Assert.Equal("/oauth/access_token", handler.LastRequest.Path);
        Assert.Contains("\"client_id\":\"id\"", handler.LastRequest.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Refreshes_the_token_within_the_margin_before_it_expires()
    {
        var handler = new FakeHttpMessageHandler()
            .RespondWith(TokenJson)
            .RespondWith("""{"access_token":"second","expires_in":3600}""");

        using var httpClient = new HttpClient(handler) { BaseAddress = TestClient.BaseAddress };
        var clock = new TestTimeProvider(DateTimeOffset.UnixEpoch);
        using var provider = new SendPulserTokenProvider(httpClient, Options, logger: null, clock);

        Assert.Equal("first", await provider.GetTokenAsync());

        // One second inside the 60 second refresh margin.
        clock.Advance(TimeSpan.FromSeconds(3600 - 59));

        Assert.Equal("second", await provider.GetTokenAsync());
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Keeps_the_token_while_it_is_comfortably_valid()
    {
        var handler = new FakeHttpMessageHandler().RespondWith(TokenJson);
        using var httpClient = new HttpClient(handler) { BaseAddress = TestClient.BaseAddress };
        var clock = new TestTimeProvider(DateTimeOffset.UnixEpoch);
        using var provider = new SendPulserTokenProvider(httpClient, Options, logger: null, clock);

        await provider.GetTokenAsync();
        clock.Advance(TimeSpan.FromMinutes(30));
        await provider.GetTokenAsync();

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Requests_a_single_token_when_callers_race()
    {
        var handler = new FakeHttpMessageHandler().RespondWith(TokenJson);
        using var httpClient = new HttpClient(handler) { BaseAddress = TestClient.BaseAddress };
        using var provider = new SendPulserTokenProvider(httpClient, Options);

        var tokens = await Task.WhenAll(Enumerable
            .Range(0, 8)
            .Select(_ => provider.GetTokenAsync().AsTask()));

        Assert.All(tokens, token => Assert.Equal("first", token));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Reports_refused_credentials_as_an_authentication_failure()
    {
        var handler = new FakeHttpMessageHandler().RespondWith(
            """{"error":"invalid_client","error_description":"bad credentials"}""",
            HttpStatusCode.Unauthorized);

        using var httpClient = new HttpClient(handler) { BaseAddress = TestClient.BaseAddress };
        using var provider = new SendPulserTokenProvider(httpClient, Options);

        var exception = await Assert.ThrowsAsync<SendPulserAuthenticationException>(
            () => provider.GetTokenAsync().AsTask());

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task Reports_a_token_response_without_a_token()
    {
        var handler = new FakeHttpMessageHandler().RespondWith("""{"expires_in":3600}""");
        using var httpClient = new HttpClient(handler) { BaseAddress = TestClient.BaseAddress };
        using var provider = new SendPulserTokenProvider(httpClient, Options);

        await Assert.ThrowsAsync<SendPulserAuthenticationException>(() => provider.GetTokenAsync().AsTask());
    }

    [Fact]
    public void Rejects_options_without_credentials()
    {
        using var httpClient = new HttpClient { BaseAddress = TestClient.BaseAddress };

        Assert.Throws<ArgumentException>(() =>
            new SendPulserTokenProvider(httpClient, new SendPulserOptions { ClientId = "id" }));
    }
}
