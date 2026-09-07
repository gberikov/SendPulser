using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SendPulser;

/// <summary>
/// Attaches the SendPulse bearer token to every request and replays a request once when the token was
/// rejected, which happens when it is revoked on the SendPulse side before it expires.
/// </summary>
public sealed class SendPulserAuthenticationHandler : DelegatingHandler
{
    private readonly SendPulserTokenProvider _tokenProvider;
    private readonly ILogger _logger;

    /// <summary>Creates the handler.</summary>
    /// <param name="tokenProvider">Provider of the access token.</param>
    /// <param name="logger">Logger; defaults to no logging.</param>
    public SendPulserAuthenticationHandler(
        SendPulserTokenProvider tokenProvider,
        ILogger<SendPulserAuthenticationHandler>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);

        _tokenProvider = tokenProvider;
        _logger = logger ?? NullLogger<SendPulserAuthenticationHandler>.Instance;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode is not HttpStatusCode.Unauthorized)
        {
            return response;
        }

        // The query string is left out on purpose: SendPulse query strings carry email addresses, which do
        // not belong in logs. HttpClient has already resolved the URI against the base address here.
        Log.TokenRejected(
            _logger,
            request.Method,
            request.RequestUri is null ? null : Internal.SendPulserApi.SafePath(request.RequestUri.AbsolutePath));

        response.Dispose();
        _tokenProvider.Invalidate(token);

        var refreshed = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        using var retry = await CloneAsync(request, cancellationToken).ConfigureAwait(false);
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed);

        return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<HttpRequestMessage> CloneAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy,
        };

        if (request.Content is not null)
        {
            // The original content stream is already consumed, so it is buffered before the replay.
            var buffered = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var content = new ByteArrayContent(buffered);
            foreach (var header in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in (IDictionary<string, object?>)request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }
}
