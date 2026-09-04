using SendPulser.AddressBooks;
using SendPulser.Balance;
using SendPulser.Blacklist;
using SendPulser.Campaigns;
using SendPulser.EmailAddresses;
using SendPulser.Internal;
using SendPulser.Senders;
using SendPulser.Services;
using SendPulser.Smtp;
using SendPulser.Tags;
using SendPulser.Templates;
using SendPulser.Webhooks;

namespace SendPulser;

/// <summary>
/// Client for the SendPulse email API.
/// </summary>
/// <remarks>
/// In an application with a service container, register the client with
/// <c>services.AddSendPulser(...)</c> from the <c>SendPulser.DependencyInjection</c> package so it runs on
/// a pooled <c>IHttpClientFactory</c> handler. The constructor taking
/// <see cref="SendPulserOptions"/> exists for scripts, tests and console tools; it owns the
/// <see cref="HttpClient"/> it creates and must be disposed.
/// </remarks>
public sealed class SendPulserClient : ISendPulserClient, IDisposable
{
    private readonly HttpClient? _ownedHttpClient;
    private readonly SendPulserTokenProvider? _ownedTokenProvider;
    private readonly HttpClient? _ownedTokenHttpClient;
    private bool _disposed;

    /// <summary>
    /// Creates a client over a pre-configured <see cref="HttpClient"/>, which must carry the SendPulse
    /// base address and is expected to run through a <see cref="SendPulserAuthenticationHandler"/>. This is
    /// the constructor the dependency injection package uses; the client does not dispose the supplied instance.
    /// </summary>
    /// <param name="httpClient">Authenticated HTTP client.</param>
    /// <exception cref="ArgumentException">The client has no base address.</exception>
    public SendPulserClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        if (httpClient.BaseAddress is null)
        {
            throw new ArgumentException(
                "The HttpClient needs a BaseAddress, for example " + SendPulserOptions.DefaultBaseUrl + ".",
                nameof(httpClient));
        }

        var api = new SendPulserApi(httpClient);
        AddressBooks = new AddressBookService(api);
        EmailAddresses = new EmailAddressService(api);
        Templates = new TemplateService(api);
        Campaigns = new CampaignService(api);
        Senders = new SenderService(api);
        Blacklist = new BlacklistService(api);
        Tags = new TagService(api);
        Balance = new BalanceService(api);
        Smtp = new SmtpService(api);
        Webhooks = new WebhookService(api);
    }

    /// <summary>
    /// Creates a self-contained client, including the HTTP pipeline and the token provider.
    /// </summary>
    /// <param name="options">Credentials and endpoint configuration.</param>
    /// <exception cref="ArgumentException">The options are incomplete.</exception>
    public SendPulserClient(SendPulserOptions options)
        : this(BuildPipeline(options, out var ownedHttpClient, out var tokenProvider, out var tokenHttpClient))
    {
        _ownedHttpClient = ownedHttpClient;
        _ownedTokenProvider = tokenProvider;
        _ownedTokenHttpClient = tokenHttpClient;
    }

    /// <summary>
    /// Creates a self-contained client from credentials.
    /// </summary>
    /// <param name="clientId">OAuth client ID.</param>
    /// <param name="clientSecret">OAuth client secret.</param>
    public SendPulserClient(string clientId, string clientSecret)
        : this(new SendPulserOptions { ClientId = clientId, ClientSecret = clientSecret })
    {
    }

    /// <inheritdoc />
    public IAddressBookService AddressBooks { get; }

    /// <inheritdoc />
    public IEmailAddressService EmailAddresses { get; }

    /// <inheritdoc />
    public ITemplateService Templates { get; }

    /// <inheritdoc />
    public ICampaignService Campaigns { get; }

    /// <inheritdoc />
    public ISenderService Senders { get; }

    /// <inheritdoc />
    public IBlacklistService Blacklist { get; }

    /// <inheritdoc />
    public ITagService Tags { get; }

    /// <inheritdoc />
    public IBalanceService Balance { get; }

    /// <inheritdoc />
    public ISmtpService Smtp { get; }

    /// <inheritdoc />
    public IWebhookService Webhooks { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _ownedHttpClient?.Dispose();
        _ownedTokenProvider?.Dispose();
        _ownedTokenHttpClient?.Dispose();
    }

    private static HttpClient BuildPipeline(
        SendPulserOptions options,
        out HttpClient ownedHttpClient,
        out SendPulserTokenProvider tokenProvider,
        out HttpClient tokenHttpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        var baseAddress = options.GetNormalizedBaseAddress();

        // The token request must bypass the authentication handler, otherwise it authenticates itself.
        tokenHttpClient = new HttpClient { BaseAddress = baseAddress, Timeout = options.Timeout };
        tokenProvider = new SendPulserTokenProvider(tokenHttpClient, options);

        var handler = new SendPulserAuthenticationHandler(tokenProvider)
        {
            InnerHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) },
        };

        ownedHttpClient = new HttpClient(handler) { BaseAddress = baseAddress, Timeout = options.Timeout };
        return ownedHttpClient;
    }
}
