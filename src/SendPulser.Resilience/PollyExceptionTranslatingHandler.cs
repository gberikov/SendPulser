using Polly;
using Polly.RateLimiting;
using Polly.Timeout;

namespace SendPulser.Resilience;

/// <summary>
/// Turns the exceptions Polly throws when the pipeline gives up into <see cref="SendPulserTransportException"/>,
/// so callers deal with one exception family whether or not the resilience package is installed.
/// </summary>
internal sealed class PollyExceptionTranslatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutRejectedException exception)
        {
            throw new SendPulserTransportException(
                $"The request {request.Method} {request.RequestUri?.AbsolutePath} to SendPulse timed out.",
                exception);
        }
        catch (RateLimiterRejectedException exception)
        {
            throw new SendPulserTransportException(
                $"The request {request.Method} {request.RequestUri?.AbsolutePath} was rejected by the client-side rate limiter: too many requests are already waiting.",
                exception);
        }
        catch (ExecutionRejectedException exception)
        {
            throw new SendPulserTransportException(
                $"The request {request.Method} {request.RequestUri?.AbsolutePath} to SendPulse was rejected by the resilience pipeline.",
                exception);
        }
    }
}
