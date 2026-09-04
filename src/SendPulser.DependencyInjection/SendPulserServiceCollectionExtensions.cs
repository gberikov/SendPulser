using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendPulser;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the SendPulse client on an <see cref="IServiceCollection"/>.
/// </summary>
public static class SendPulserServiceCollectionExtensions
{
    /// <summary>
    /// Name of the <see cref="HttpClient"/> the API calls run on.
    /// </summary>
    public const string HttpClientName = "SendPulser";

    /// <summary>
    /// Name of the <see cref="HttpClient"/> the OAuth token request runs on. It deliberately skips the
    /// authentication handler.
    /// </summary>
    public const string TokenHttpClientName = "SendPulser.Token";

    /// <summary>
    /// Registers <see cref="ISendPulserClient"/> and its HTTP pipeline.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Callback that fills in credentials and endpoint settings.</param>
    /// <returns>The builder of the API <see cref="HttpClient"/>, so policies can be added to it.</returns>
    public static IHttpClientBuilder AddSendPulser(
        this IServiceCollection services,
        Action<SendPulserOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<SendPulserOptions>().Configure(configure);
        return AddCore(services);
    }

    /// <summary>
    /// Registers <see cref="ISendPulserClient"/> and binds its options to a configuration section.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">
    /// Section holding <c>ClientId</c>, <c>ClientSecret</c> and optionally <c>BaseUrl</c> and
    /// <c>Timeout</c>, by convention the section named <see cref="SendPulserOptions.SectionName"/>.
    /// </param>
    /// <returns>The builder of the API <see cref="HttpClient"/>, so policies can be added to it.</returns>
    /// <remarks>
    /// Values are read key by key rather than through <c>Bind</c>, which relies on reflection and would
    /// make the package unusable under Native AOT.
    /// </remarks>
    public static IHttpClientBuilder AddSendPulser(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddSendPulser(options =>
        {
            if (configuration[nameof(SendPulserOptions.ClientId)] is { Length: > 0 } clientId)
            {
                options.ClientId = clientId;
            }

            if (configuration[nameof(SendPulserOptions.ClientSecret)] is { Length: > 0 } clientSecret)
            {
                options.ClientSecret = clientSecret;
            }

            if (configuration["BaseUrl"] is { Length: > 0 } baseUrl)
            {
                options.BaseAddress = new Uri(baseUrl);
            }

            if (TimeSpan.TryParse(configuration[nameof(SendPulserOptions.Timeout)], out var timeout))
            {
                options.Timeout = timeout;
            }
        });
    }

    private static IHttpClientBuilder AddCore(IServiceCollection services)
    {
        services.PostConfigure<SendPulserOptions>(options => options.Validate());

        // Both clients are long lived on purpose: the token provider is a singleton and the handler is
        // therefore never rotated, so connection recycling is delegated to PooledConnectionLifetime.
        services.AddHttpClient(TokenHttpClientName, ConfigureHttpClient)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            });

        services.TryAddSingleton(provider => new SendPulserTokenProvider(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(TokenHttpClientName),
            provider.GetRequiredService<IOptions<SendPulserOptions>>().Value,
            provider.GetService<ILogger<SendPulserTokenProvider>>(),
            provider.GetService<TimeProvider>()));

        services.TryAddTransient(provider => new SendPulserAuthenticationHandler(
            provider.GetRequiredService<SendPulserTokenProvider>(),
            provider.GetService<ILogger<SendPulserAuthenticationHandler>>()));

        services.TryAddTransient<ISendPulserClient>(provider => new SendPulserClient(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        return services.AddHttpClient(HttpClientName, ConfigureHttpClient)
            .AddHttpMessageHandler<SendPulserAuthenticationHandler>();
    }

    private static void ConfigureHttpClient(IServiceProvider provider, HttpClient client)
    {
        var options = provider.GetRequiredService<IOptions<SendPulserOptions>>().Value;
        client.BaseAddress = options.GetNormalizedBaseAddress();
        client.Timeout = options.Timeout;
    }
}
