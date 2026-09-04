using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SendPulser.Internal;

namespace SendPulser;

/// <summary>
/// Obtains and caches the OAuth access token of a SendPulse account.
/// </summary>
/// <remarks>
/// Tokens live for one hour and SendPulse allows several of them to be valid at once, so the cache is
/// per instance and deliberately not shared between processes. Concurrent callers wait for a single
/// refresh rather than each requesting their own token.
/// </remarks>
public sealed class SendPulserTokenProvider : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SendPulserOptions _options;
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _accessToken;
    private DateTimeOffset _expiresAt;
    private bool _disposed;

    /// <summary>
    /// Creates a provider.
    /// </summary>
    /// <param name="httpClient">
    /// Client used for the token request. It must not run through
    /// <see cref="SendPulserAuthenticationHandler"/>, otherwise the token request authenticates itself.
    /// </param>
    /// <param name="options">Credentials and refresh margin.</param>
    /// <param name="logger">Logger; defaults to no logging.</param>
    /// <param name="timeProvider">Clock; defaults to the system clock.</param>
    public SendPulserTokenProvider(
        HttpClient httpClient,
        SendPulserOptions options,
        ILogger<SendPulserTokenProvider>? logger = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        _httpClient = httpClient;
        _options = options;
        _logger = logger ?? NullLogger<SendPulserTokenProvider>.Instance;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Returns a valid access token, requesting a new one when the cached token is missing or about to expire.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A bearer token.</returns>
    /// <exception cref="SendPulserAuthenticationException">SendPulse refused the credentials.</exception>
    public async ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetCachedToken(out var cached))
        {
            return cached;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (TryGetCachedToken(out cached))
            {
                return cached;
            }

            return await RequestTokenAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <summary>
    /// Drops the cached token so the next call requests a fresh one.
    /// </summary>
    public void Invalidate()
    {
        _accessToken = null;
        _expiresAt = default;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _refreshLock.Dispose();
    }

    private bool TryGetCachedToken(out string token)
    {
        var current = _accessToken;
        if (current is not null && _timeProvider.GetUtcNow() < _expiresAt)
        {
            token = current;
            return true;
        }

        token = string.Empty;
        return false;
    }

    private async Task<string> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var request = new TokenRequest
        {
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret,
        };

        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.TokenRequest);
        using var response = await _httpClient
            .PostAsync(new Uri("oauth/access_token", UriKind.Relative), content, cancellationToken)
            .ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new SendPulserAuthenticationException(
                $"SendPulse refused the credentials with HTTP {(int)response.StatusCode}.",
                response.StatusCode,
                errorCode: null,
                body);
        }

        TokenResponse? token;
        try
        {
            token = JsonSerializer.Deserialize(body, SendPulserJsonContext.Default.TokenResponse);
        }
        catch (JsonException exception)
        {
            throw new SendPulserAuthenticationException(
                "SendPulse returned a token response that could not be parsed.",
                exception);
        }

        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            throw new SendPulserAuthenticationException(
                "SendPulse returned a token response without an access token.",
                response.StatusCode,
                errorCode: null,
                body);
        }

        var lifetime = TimeSpan.FromSeconds(token.ExpiresIn);
        var margin = _options.TokenRefreshMargin < lifetime ? _options.TokenRefreshMargin : TimeSpan.Zero;

        _accessToken = token.AccessToken;
        _expiresAt = _timeProvider.GetUtcNow() + lifetime - margin;

        Log.TokenObtained(_logger, token.ExpiresIn);
        return token.AccessToken;
    }
}
