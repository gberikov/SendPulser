namespace SendPulser;

/// <summary>
/// Configuration for the SendPulse API client.
/// </summary>
public sealed class SendPulserOptions
{
    /// <summary>Configuration section name used by the DI package.</summary>
    public const string SectionName = "SendPulser";

    /// <summary>Default SendPulse API base address.</summary>
    public const string DefaultBaseUrl = "https://api.sendpulse.com/";

    /// <summary>
    /// OAuth client ID, taken from the API tab of the SendPulse account settings.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client secret, taken from the API tab of the SendPulse account settings.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Base address of the API. Defaults to <see cref="DefaultBaseUrl"/>. Must use HTTPS unless it points
    /// at the local machine, because the client secret travels in the token request body.
    /// </summary>
    public Uri BaseAddress { get; set; } = new(DefaultBaseUrl);

    /// <summary>
    /// How long before actual expiry the access token is refreshed. Defaults to one minute;
    /// SendPulse tokens live for one hour.
    /// </summary>
    public TimeSpan TokenRefreshMargin { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Timeout applied to the underlying <see cref="System.Net.Http.HttpClient"/>. Defaults to 100 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Throws when the options are not usable.
    /// </summary>
    /// <exception cref="ArgumentException">A required value is missing or malformed.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new ArgumentException("SendPulser: ClientId is required.", nameof(ClientId));
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new ArgumentException("SendPulser: ClientSecret is required.", nameof(ClientSecret));
        }

        if (BaseAddress is null || !BaseAddress.IsAbsoluteUri)
        {
            throw new ArgumentException("SendPulser: BaseAddress must be an absolute URI.", nameof(BaseAddress));
        }

        if (BaseAddress.Scheme != Uri.UriSchemeHttps && !BaseAddress.IsLoopback)
        {
            throw new ArgumentException(
                "SendPulser: BaseAddress must use https, the client secret is sent in the token request.",
                nameof(BaseAddress));
        }

        if (TokenRefreshMargin < TimeSpan.Zero)
        {
            throw new ArgumentException("SendPulser: TokenRefreshMargin cannot be negative.", nameof(TokenRefreshMargin));
        }

        if (Timeout <= TimeSpan.Zero && Timeout != System.Threading.Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentException("SendPulser: Timeout must be positive.", nameof(Timeout));
        }
    }

    /// <summary>
    /// Returns the base address with a guaranteed trailing slash, so relative request paths compose correctly.
    /// </summary>
    public Uri GetNormalizedBaseAddress() =>
        BaseAddress.AbsoluteUri.EndsWith('/') ? BaseAddress : new Uri(BaseAddress.AbsoluteUri + "/");
}
