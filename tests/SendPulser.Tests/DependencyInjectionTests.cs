using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void Registers_a_resolvable_client()
    {
        var services = new ServiceCollection();
        services.AddSendPulser(options =>
        {
            options.ClientId = "id";
            options.ClientSecret = "secret";
        });

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<ISendPulserClient>());
    }

    [Fact]
    public void Reads_credentials_from_configuration_without_reflection_binding()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SendPulser:ClientId"] = "from-config",
                ["SendPulser:ClientSecret"] = "secret",
                ["SendPulser:BaseUrl"] = "https://example.test/api",
                ["SendPulser:Timeout"] = "00:00:42",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSendPulser(configuration.GetSection(SendPulserOptions.SectionName));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SendPulserOptions>>().Value;

        Assert.Equal("from-config", options.ClientId);
        Assert.Equal(new Uri("https://example.test/api"), options.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(42), options.Timeout);
    }

    [Fact]
    public void Fails_fast_when_the_credentials_are_incomplete()
    {
        var services = new ServiceCollection();
        services.AddSendPulser(options => options.ClientId = "id");

        using var provider = services.BuildServiceProvider();

        Assert.Throws<ArgumentException>(() => provider.GetRequiredService<IOptions<SendPulserOptions>>().Value);
    }

    [Fact]
    public async Task Sends_authenticated_requests_through_the_registered_pipeline()
    {
        var apiHandler = new FakeHttpMessageHandler()
            .RespondWith(TestClient.Fixture("address-books.json"));
        var tokenHandler = new FakeHttpMessageHandler()
            .RespondWith("""{"access_token":"token-from-di","expires_in":3600}""");

        var services = new ServiceCollection();
        services.AddSendPulser(options =>
        {
            options.ClientId = "id";
            options.ClientSecret = "secret";
        });

        UsePrimaryHandler(services, SendPulserServiceCollectionExtensions.HttpClientName, apiHandler);
        UsePrimaryHandler(services, SendPulserServiceCollectionExtensions.TokenHttpClientName, tokenHandler);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ISendPulserClient>();

        var books = await client.AddressBooks.GetAllAsync();

        Assert.Equal(2, books.Count);
        Assert.Equal("token-from-di", apiHandler.LastRequest.BearerToken);
        Assert.Equal("https://api.sendpulse.com/addressbooks", apiHandler.LastRequest.Uri?.ToString());
    }

    [Fact]
    public async Task Retries_a_get_after_a_server_error_but_never_a_post()
    {
        var apiHandler = new FakeHttpMessageHandler()
            .RespondWith("{}", HttpStatusCode.InternalServerError)
            .RespondWith(TestClient.Fixture("address-books.json"))
            .RespondWith("{}", HttpStatusCode.InternalServerError);

        var tokenHandler = new FakeHttpMessageHandler()
            .RespondWith("""{"access_token":"token","expires_in":3600}""");

        var services = new ServiceCollection();
        services
            .AddSendPulser(options =>
            {
                options.ClientId = "id";
                options.ClientSecret = "secret";
            })
            .AddSendPulserResilience(options => options.RetryDelay = TimeSpan.FromMilliseconds(1));

        UsePrimaryHandler(services, SendPulserServiceCollectionExtensions.HttpClientName, apiHandler);
        UsePrimaryHandler(services, SendPulserServiceCollectionExtensions.TokenHttpClientName, tokenHandler);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ISendPulserClient>();

        var books = await client.AddressBooks.GetAllAsync();
        Assert.Equal(2, books.Count);
        Assert.Equal(2, apiHandler.Requests.Count);

        // Creating a mailing list is a write: a replay would create it twice, so it is not retried.
        await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.CreateAsync("book"));
        Assert.Equal(3, apiHandler.Requests.Count);
    }

    private static void UsePrimaryHandler(IServiceCollection services, string name, HttpMessageHandler handler) =>
        services.Configure<HttpClientFactoryOptions>(
            name,
            options => options.HttpMessageHandlerBuilderActions.Add(
                builder => builder.PrimaryHandler = handler));
}
