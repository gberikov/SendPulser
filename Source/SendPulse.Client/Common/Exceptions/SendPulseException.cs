namespace SendPulse.Client.Common.Exceptions;

/// <summary>
/// Exception thrown when SendPulse API returns an error
/// </summary>
public class SendPulseException : Exception
{
    public SendPulseException()
    {
    }

    public SendPulseException(string message) : base(message)
    {
    }

    public SendPulseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

