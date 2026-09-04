using System.Net;

namespace SendPulser;

/// <summary>
/// Raised when the SendPulse API reports a failure, either through an HTTP status code or through an
/// <c>is_error</c> payload returned with HTTP 200.
/// </summary>
public class SendPulserApiException : SendPulserException
{
    /// <summary>Creates an empty exception.</summary>
    public SendPulserApiException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    /// <param name="message">Error description.</param>
    public SendPulserApiException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    /// <param name="message">Error description.</param>
    /// <param name="innerException">The cause.</param>
    public SendPulserApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates a fully described API failure.</summary>
    /// <param name="message">Error description.</param>
    /// <param name="statusCode">HTTP status code of the response.</param>
    /// <param name="errorCode">SendPulse specific error code, when the payload carried one.</param>
    /// <param name="responseBody">Raw response body, kept for diagnostics.</param>
    public SendPulserApiException(string message, HttpStatusCode statusCode, int? errorCode, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ResponseBody = responseBody;
    }

    /// <summary>HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// SendPulse error code, when present. Code <c>2020202020</c> means the hard limit of
    /// 10 requests per second was exceeded.
    /// </summary>
    public int? ErrorCode { get; }

    /// <summary>Raw response body.</summary>
    public string? ResponseBody { get; }
}
