namespace SendPulser.Campaigns;

/// <summary>
/// Status codes of a campaign. Exposed as constants rather than an enum so a code SendPulse adds later
/// still deserializes.
/// </summary>
public static class CampaignStatus
{
    /// <summary>New.</summary>
    public const int New = 0;

    /// <summary>Pending moderation.</summary>
    public const int InModeration = 1;

    /// <summary>Sending in progress.</summary>
    public const int Sending = 2;

    /// <summary>Sent.</summary>
    public const int Sent = 3;

    /// <summary>Test campaign, sent to the account owner.</summary>
    public const int Test = 4;

    /// <summary>Blocked by the service.</summary>
    public const int Blocked = 5;

    /// <summary>Marked for deletion.</summary>
    public const int MarkedForDeletion = 6;

    /// <summary>Status is being updated; <see cref="Sent"/> follows.</summary>
    public const int StatusUpdating = 7;

    /// <summary>Test campaign sent.</summary>
    public const int TestSent = 8;

    /// <summary>Delivery in progress.</summary>
    public const int Delivering = 9;

    /// <summary>Being prepared for sending.</summary>
    public const int Preparing = 10;

    /// <summary>Awaiting the account owner's answer to the moderator.</summary>
    public const int AwaitingModeratorAnswer = 11;

    /// <summary>No active addresses to send to.</summary>
    public const int NoActiveAddresses = 12;

    /// <summary>Addresses are being copied into the campaign.</summary>
    public const int Creating = 13;

    /// <summary>Queued; <see cref="Sent"/> follows when every message is out.</summary>
    public const int Queued = 14;

    /// <summary>Awaiting A/B test results.</summary>
    public const int AwaitingAbTest = 15;

    /// <summary>Cancelled by the account owner.</summary>
    public const int Cancelled = 16;

    /// <summary>Sending partially.</summary>
    public const int SendingPartially = 22;

    /// <summary>Sent partially.</summary>
    public const int SentPartially = 23;

    /// <summary>Partially sent and then blocked by the service.</summary>
    public const int PartiallySentAndBlocked = 25;

    /// <summary>Draft.</summary>
    public const int Draft = 26;

    /// <summary>Requires editing.</summary>
    public const int RequiresEditing = 27;

    /// <summary>Scheduled to be resent to recipients who did not open it.</summary>
    public const int ResendToUnreadScheduled = 28;

    /// <summary>Automation flow stopped because the balance ran out.</summary>
    public const int AutomationBalanceExceeded = 33;

    /// <summary>Automation flow draft.</summary>
    public const int AutomationDraft = 36;
}

/// <summary>
/// Delivery status codes reported per recipient in campaign statistics.
/// </summary>
public static class DeliveryStatus
{
    /// <summary>In queue.</summary>
    public const int InQueue = 0;

    /// <summary>Sent.</summary>
    public const int Sent = 1;

    /// <summary>Delivered.</summary>
    public const int Delivered = 2;

    /// <summary>Opened.</summary>
    public const int Opened = 3;

    /// <summary>A link was clicked.</summary>
    public const int Clicked = 4;

    /// <summary>The recipient unsubscribed.</summary>
    public const int Unsubscribed = 5;

    /// <summary>The receiving server reported that the mailbox does not exist.</summary>
    public const int NoSuchEmail = 6;

    /// <summary>Temporarily unavailable, will be retried.</summary>
    public const int TemporarilyUnavailable = 7;

    /// <summary>Unavailable.</summary>
    public const int Unavailable = 8;

    /// <summary>Rejected by the receiving server as spam.</summary>
    public const int RejectedAsSpam = 9;

    /// <summary>The mailbox is full.</summary>
    public const int MailboxFull = 10;

    /// <summary>Marked as spam by the recipient.</summary>
    public const int MarkedAsSpam = 11;

    /// <summary>Delivery failed.</summary>
    public const int DeliveryFailure = 12;

    /// <summary>The address is not valid.</summary>
    public const int InvalidEmail = 16;

    /// <summary>Temporarily blocked.</summary>
    public const int TemporarilyBlocked = 17;

    /// <summary>Disabled by the service.</summary>
    public const int DisabledByAdministrator = 18;

    /// <summary>Already unsubscribed.</summary>
    public const int AlreadyUnsubscribed = 20;
}
