namespace SendPulser.Smtp;

/// <summary>
/// Transactional email through the SendPulse SMTP service.
/// </summary>
/// <remarks>
/// Wraps <see href="https://sendpulse.com/integrations/api/smtp">the SMTP service API</see>. The SMTP
/// service must be activated on the account before any of these calls succeed. Sender addresses are
/// shared with the bulk email service and managed through <see cref="Senders.ISenderService"/>.
/// </remarks>
public interface ISmtpService
{
    /// <summary>
    /// Sends one transactional email. This call is not idempotent: a retry sends the message again,
    /// which is why the resilience package never retries it automatically.
    /// </summary>
    /// <param name="request">Sender, recipients, subject and body or template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID assigned to the message.</returns>
    Task<SendEmailResult> SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lists processed messages.</summary>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="fromDate">Earliest send date to include.</param>
    /// <param name="toDate">Latest send date to include.</param>
    /// <param name="sender">Filters by sender address.</param>
    /// <param name="recipient">Filters by recipient address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching messages.</returns>
    Task<IReadOnlyList<SmtpEmail>> GetEmailsAsync(
        int? limit = null,
        int? offset = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? sender = null,
        string? recipient = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a single message by the ID returned when it was sent.</summary>
    /// <param name="messageId">Message ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The message.</returns>
    /// <exception cref="SendPulserApiException">The message does not exist.</exception>
    Task<SmtpEmail> GetEmailAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>Gets several messages at once. SendPulse accepts up to 500 IDs per call.</summary>
    /// <param name="messageIds">Message IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The messages that were found.</returns>
    Task<IReadOnlyList<SmtpEmail>> GetEmailsAsync(
        IReadOnlyList<string> messageIds,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the total number of messages sent through the account.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of messages.</returns>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the messages that bounced. SendPulse keeps bounce details for the last 24 hours only.
    /// </summary>
    /// <param name="onDate">Restricts the result to a single day.</param>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bounces.</returns>
    Task<SmtpBouncePage> GetBouncesAsync(
        DateOnly? onDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the number of bounces in the last 24 hours.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of bounces.</returns>
    Task<int> GetBounceCountAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds addresses to the unsubscribe list.</summary>
    /// <param name="requests">Addresses and the reason recorded with each of them.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UnsubscribeAsync(
        IReadOnlyList<UnsubscribeRequest> requests,
        CancellationToken cancellationToken = default);

    /// <summary>Removes addresses from the unsubscribe list.</summary>
    /// <param name="emails">Addresses to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveFromUnsubscribeListAsync(
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default);

    /// <summary>Lists unsubscribed contacts.</summary>
    /// <param name="onDate">Restricts the result to a single day.</param>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unsubscribed contacts.</returns>
    Task<IReadOnlyList<UnsubscribedContact>> GetUnsubscribedAsync(
        DateOnly? onDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>Tells whether an address is on the unsubscribe list.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the address is unsubscribed.</returns>
    Task<bool> IsUnsubscribedAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Emails a resubscription request to an unsubscribed address. SendPulse allows five such emails per
    /// account per 24 hours.
    /// </summary>
    /// <param name="email">Recipient address.</param>
    /// <param name="sender">Activated sender address.</param>
    /// <param name="language">Language of the email: <c>en</c>, <c>ru</c>, <c>ua</c>, <c>tr</c>, <c>es</c> or <c>pt</c>; <c>en</c> when omitted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the email that was sent.</returns>
    Task<SendEmailResult> ResubscribeAsync(
        string email,
        string sender,
        string? language = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the sender addresses available to the SMTP service.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sender addresses.</returns>
    Task<IReadOnlyList<string>> GetSendersAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists the IP addresses SendPulse delivers the account's messages from.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The IP addresses.</returns>
    Task<IReadOnlyList<string>> GetIpAddressesAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists the domains registered for sending, with their DNS validation state.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The domains.</returns>
    Task<IReadOnlyList<SenderDomain>> GetSenderDomainsAsync(CancellationToken cancellationToken = default);

    /// <summary>Registers a sending domain. DNS records still have to be published for it to activate.</summary>
    /// <param name="domain">Domain name, for example <c>example.com</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddSenderDomainAsync(string domain, CancellationToken cancellationToken = default);
}
