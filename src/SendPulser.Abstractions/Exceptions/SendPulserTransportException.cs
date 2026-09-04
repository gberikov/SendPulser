namespace SendPulser;

/// <summary>
/// Raised when a request never produced a SendPulse response: the connection failed, an attempt timed
/// out or the client-side rate limiter rejected the call. The cause is kept in
/// <see cref="Exception.InnerException"/>.
/// </summary>
public sealed class SendPulserTransportException : SendPulserException
{
    /// <summary>Creates an empty exception.</summary>
    public SendPulserTransportException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    /// <param name="message">Error description.</param>
    public SendPulserTransportException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and the transport failure that caused it.</summary>
    /// <param name="message">Error description.</param>
    /// <param name="innerException">The cause.</param>
    public SendPulserTransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
