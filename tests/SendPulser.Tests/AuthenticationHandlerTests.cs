using System.Net;
using System.Text;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class AuthenticationHandlerTests
{
    private static SendPulserOptions Options => new() { ClientId = "id", ClientSecret = "secret" };

    [Fact]
    public async Task Attaches_the_bearer_token_to_every_request()
    {
        var tokenHandler = new FakeHttpMessageHandler()
            .RespondWith("""{"access_token":"abc","expires_in":3600}""");
        using var tokenClient = new HttpClient(tokenHandler) { BaseAddress = TestClient.BaseAddress };
        using var provider = new SendPulserTokenProvider(tokenClient, Options);

        var apiHandler = new FakeHttpMessageHandler().RespondWith("[]");
        using var client = new HttpClient(new SendPulserAuthenticationHandler(provider) { InnerHandler = apiHandler })
        {
            BaseAddress = TestClient.BaseAddress,
        };

        using var response = await client.GetAsync(new Uri("addressbooks", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("abc", apiHandler.LastRequest.BearerToken);
    }

    [Fact]
    public async Task Refreshes_the_token_and_replays_the_request_once_after_a_401()
    {
        var tokenHandler = new FakeHttpMessageHandler()
            .RespondWith("""{"access_token":"stale","expires_in":3600}""")
            .RespondWith("""{"access_token":"fresh","expires_in":3600}""");
        using var tokenClient = new HttpClient(tokenHandler) { BaseAddress = TestClient.BaseAddress };
        using var provider = new SendPulserTokenProvider(tokenClient, Options);

        var apiHandler = new FakeHttpMessageHandler()
            .RespondWith("""{"message":"expired"}""", HttpStatusCode.Unauthorized)
            .RespondWith("""{"result":true}""");

        using var client = new HttpClient(new SendPulserAuthenticationHandler(provider) { InnerHandler = apiHandler })
        {
            BaseAddress = TestClient.BaseAddress,
        };

        using var content = new StringContent("""{"bookName":"x"}""", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(new Uri("addressbooks", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, apiHandler.Requests.Count);
        Assert.Equal("stale", apiHandler.Requests[0].BearerToken);
        Assert.Equal("fresh", apiHandler.Requests[1].BearerToken);

        // The replay must carry the original body, not an empty one.
        Assert.Equal("""{"bookName":"x"}""", apiHandler.Requests[1].Body);
    }

    [Fact]
    public async Task Gives_up_after_a_single_replay()
    {
        var tokenHandler = new FakeHttpMessageHandler()
            .RespondWith("""{"access_token":"one","expires_in":3600}""")
            .RespondWith("""{"access_token":"two","expires_in":3600}""");
        using var tokenClient = new HttpClient(tokenHandler) { BaseAddress = TestClient.BaseAddress };
        using var provider = new SendPulserTokenProvider(tokenClient, Options);

        var apiHandler = new FakeHttpMessageHandler()
            .RespondWith("{}", HttpStatusCode.Unauthorized)
            .RespondWith("{}", HttpStatusCode.Unauthorized);

        using var client = new HttpClient(new SendPulserAuthenticationHandler(provider) { InnerHandler = apiHandler })
        {
            BaseAddress = TestClient.BaseAddress,
        };

        using var response = await client.GetAsync(new Uri("addressbooks", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(2, apiHandler.Requests.Count);
    }
}
