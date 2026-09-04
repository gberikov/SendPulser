namespace SendPulser.AddressBooks;

/// <summary>
/// Status codes SendPulse assigns to an email address in a mailing list. The codes are exposed as
/// constants rather than an enum so a value SendPulse adds later still deserializes.
/// </summary>
public static class ContactStatus
{
    /// <summary>Newly added; delivery is blocked until the service activates the address.</summary>
    public const int New = 0;

    /// <summary>Delivery is allowed.</summary>
    public const int Active = 1;

    /// <summary>A confirmation email was sent to the address holder.</summary>
    public const int ConfirmationRequested = 2;

    /// <summary>Pending activation by the service.</summary>
    public const int ActivationRequested = 3;

    /// <summary>The recipient unsubscribed from this sender.</summary>
    public const int Unsubscribed = 4;

    /// <summary>Rejected by the service.</summary>
    public const int RejectedByAdmin = 5;

    /// <summary>Unsubscribed from every mailing list of the sender, or blocked after complaints.</summary>
    public const int UnsubscribedFromAll = 6;

    /// <summary>An activation email was sent to the recipient.</summary>
    public const int ActivationEmailSent = 7;

    /// <summary>Blocked by the account owner.</summary>
    public const int BlockedByUser = 8;

    /// <summary>Delivery to this address keeps failing.</summary>
    public const int SendingError = 9;

    /// <summary>Blocked by the host list.</summary>
    public const int BlockedByHost = 10;

    /// <summary>Blocked by the sender name.</summary>
    public const int BlockedBySenderName = 11;

    /// <summary>Blocked by a part of the address.</summary>
    public const int BlockedByAddressPart = 12;

    /// <summary>Deleted by the account owner.</summary>
    public const int DeletedByUser = 13;

    /// <summary>Temporarily unavailable.</summary>
    public const int TemporarilyUnavailable = 14;
}
