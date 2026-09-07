using System.Net;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using Polly.Timeout;
using SendPulser.Resilience;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Adds retry, rate limiting and timeout policies to the SendPulser HTTP pipeline.
/// </summary>
public static class SendPulserResilienceHttpClientBuilderExtensions
{
    /// <summary>
    /// Adds the SendPulser resilience pipeline to the client returned by <c>AddSendPulser</c>.
    /// </summary>
    /// <param name="builder">Builder of the SendPulser HTTP client.</param>
    /// <param name="configure">Callback that overrides the defaults.</param>
    /// <returns>The same builder.</returns>
    /// <remarks>
    /// Writes are never retried automatically. Sending a campaign or a transactional email is not
    /// idempotent, so a retried <c>POST</c> can deliver the same email twice; only <c>GET</c> and
    /// <c>DELETE</c> are replayed on transient failures. HTTP 429 is the exception: the request was
    /// rejected rather than processed, so it is retried for every method, waiting for the interval named
    /// in the <c>Retry-After</c> header. When the pipeline gives up, the failure surfaces as a
    /// <see cref="SendPulser.SendPulserTransportException"/> rather than a Polly exception.
    /// </remarks>
    public static IHttpClientBuilder AddSendPulserResilience(
        this IHttpClientBuilder builder,
        Action<SendPulserResilienceOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new SendPulserResilienceOptions();
        configure?.Invoke(options);

        // Registered before the resilience handler so it sits outside it and sees what the pipeline throws.
        builder.AddHttpMessageHandler(() => new PollyExceptionTranslatingHandler());

        builder.AddResilienceHandler("sendpulser", pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetryAttempts,
                Delay = options.RetryDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldRetryAfterHeader = true,
                ShouldHandle = arguments => ValueTask.FromResult(ShouldRetry(arguments)),
            });

            // Each retry is a separate network request and must acquire its own permit.
            pipeline.AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = CreateRateLimiter(options),
            });

            pipeline.AddTimeout(options.AttemptTimeout);
        });

        return builder;
    }

    private static Func<RateLimiterArguments, ValueTask<RateLimitLease>> CreateRateLimiter(
        SendPulserResilienceOptions options)
    {
        var limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = options.RequestsPerSecond,
            Window = TimeSpan.FromSeconds(1),
            SegmentsPerWindow = 10,
            QueueLimit = options.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        });

        return arguments => limiter.AcquireAsync(permitCount: 1, arguments.Context.CancellationToken);
    }

    private static bool ShouldRetry(RetryPredicateArguments<HttpResponseMessage> arguments)
    {
        if (arguments.Outcome.Result is { StatusCode: HttpStatusCode.TooManyRequests })
        {
            return true;
        }

        var method = arguments.Context.GetRequestMessage()?.Method;
        if (method != HttpMethod.Get && method != HttpMethod.Delete)
        {
            return false;
        }

        // The attempt timeout surfaces as TimeoutRejectedException, which is not a TimeoutException.
        return arguments.Outcome.Exception is HttpRequestException or TimeoutException or TimeoutRejectedException
            || arguments.Outcome.Result is { StatusCode: >= HttpStatusCode.InternalServerError }
            || arguments.Outcome.Result is { StatusCode: HttpStatusCode.RequestTimeout };
    }
}
