using System.Net;
using Microsoft.Extensions.Logging;

namespace SendPulser.AspNetCore.Internal;

internal static partial class WebhookLog
{
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Debug,
        Message = "Handled a SendPulse {Family} webhook batch of {EventCount} events.")]
    public static partial void Handled(ILogger logger, string family, int eventCount);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "Rejected a SendPulse {Family} webhook request from {RemoteAddress}: the secret did not match or the address is not allowed.")]
    public static partial void Rejected(ILogger logger, string family, IPAddress? remoteAddress);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Warning,
        Message = "A SendPulse {Family} webhook request carried a body that is not valid JSON.")]
    public static partial void MalformedPayload(ILogger logger, string family, Exception exception);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Error,
        Message = "The handler for a SendPulse {Family} webhook batch of {EventCount} events failed; answering 500 so the failure is visible.")]
    public static partial void HandlerFailed(ILogger logger, string family, int eventCount, Exception exception);
}
