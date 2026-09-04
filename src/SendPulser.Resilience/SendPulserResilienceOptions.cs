namespace SendPulser.Resilience;

/// <summary>
/// Tuning knobs for the SendPulser resilience pipeline. The defaults follow the limits SendPulse
/// documents: a hard ceiling of ten requests per second and HTTP 429 once the per minute quota of the
/// plan is used up.
/// </summary>
public sealed class SendPulserResilienceOptions
{
    /// <summary>
    /// Requests per second allowed through the client side limiter. Defaults to the SendPulse ceiling of
    /// ten; lower it when several processes share one account.
    /// </summary>
    public int RequestsPerSecond { get; set; } = RateLimit.RequestsPerSecond;

    /// <summary>
    /// How many requests may wait for a slot before the limiter starts rejecting them. Defaults to 64.
    /// </summary>
    public int QueueLimit { get; set; } = 64;

    /// <summary>
    /// Maximum number of retries. Defaults to three.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay of the exponential backoff. Defaults to one second, with jitter applied on top.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Timeout of a single attempt. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
