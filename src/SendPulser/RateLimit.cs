namespace SendPulser;

/// <summary>
/// Limits SendPulse enforces on API traffic.
/// </summary>
public static class RateLimit
{
    /// <summary>
    /// Hard ceiling of requests per second, enforced on every plan.
    /// </summary>
    public const int RequestsPerSecond = 10;

    /// <summary>
    /// Proprietary error code returned when <see cref="RequestsPerSecond"/> is exceeded.
    /// </summary>
    public const int PerSecondErrorCode = 2020202020;
}
