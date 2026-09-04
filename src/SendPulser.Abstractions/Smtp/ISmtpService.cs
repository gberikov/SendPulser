namespace SendPulser.Smtp;

/// <summary>
/// Transactional email through the SendPulse SMTP service.
/// </summary>
/// <remarks>
/// Wraps <see href="https://sendpulse.com/integrations/api/smtp">the SMTP service API</see>. The SMTP
/// service must be activated on the account before any of these calls succeed.
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

    /// <summary>Gets the total number of messages sent through the account.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of messages.</returns>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

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

    /// <summary>Lists the sender addresses available to the SMTP service.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sender addresses.</returns>
    Task<IReadOnlyList<string>> GetSendersAsync(CancellationToken cancellationToken = default);
}
