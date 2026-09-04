using System.Net;

namespace SendPulser;

/// <summary>
/// Raised when an OAuth access token could not be obtained.
/// </summary>
public sealed class SendPulserAuthenticationException : SendPulserApiException
{
    /// <summary>Creates an empty exception.</summary>
    public SendPulserAuthenticationException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    /// <param name="message">Error description.</param>
    public SendPulserAuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    /// <param name="message">Error description.</param>
    /// <param name="innerException">The cause.</param>
    public SendPulserAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates a fully described authentication failure.</summary>
    /// <param name="message">Error description.</param>
    /// <param name="statusCode">HTTP status code of the token response.</param>
    /// <param name="errorCode">SendPulse specific error code, when the payload carried one.</param>
    /// <param name="responseBody">Raw response body, kept for diagnostics.</param>
    public SendPulserAuthenticationException(string message, HttpStatusCode statusCode, int? errorCode, string? responseBody)
        : base(message, statusCode, errorCode, responseBody)
    {
    }
}
