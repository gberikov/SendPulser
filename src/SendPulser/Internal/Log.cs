using Microsoft.Extensions.Logging;

namespace SendPulser;

/// <summary>
/// Structured log messages emitted by the client. Request and response logging is left to
/// <c>IHttpClientFactory</c>, which already logs both.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "SendPulser obtained a new access token, valid for {ExpiresInSeconds} seconds.")]
    public static partial void TokenObtained(ILogger logger, int expiresInSeconds);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "SendPulse rejected the access token for {Method} {Uri}; refreshing and retrying once.")]
    public static partial void TokenRejected(ILogger logger, HttpMethod method, Uri? uri);
}
