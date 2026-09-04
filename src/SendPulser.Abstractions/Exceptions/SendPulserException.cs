namespace SendPulser;

/// <summary>
/// Base type for every error raised by SendPulser.
/// </summary>
public class SendPulserException : Exception
{
    /// <summary>Creates an empty exception.</summary>
    public SendPulserException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    /// <param name="message">Error description.</param>
    public SendPulserException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    /// <param name="message">Error description.</param>
    /// <param name="innerException">The cause.</param>
    public SendPulserException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
