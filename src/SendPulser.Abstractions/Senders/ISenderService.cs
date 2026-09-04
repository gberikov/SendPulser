namespace SendPulser.Senders;

/// <summary>
/// Sender addresses of the account. Campaigns and transactional emails can only be sent from an
/// activated sender.
/// </summary>
/// <remarks>
/// Wraps the <c>/senders</c> endpoints of
/// <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>. The
/// same list serves the SMTP service, see <see cref="Sender.IsAllowedForSmtp"/>.
/// </remarks>
public interface ISenderService
{
    /// <summary>Lists the sender addresses of the account.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The senders.</returns>
    Task<IReadOnlyList<Sender>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a sender address. SendPulse emails an activation code to it; pass the code to
    /// <see cref="ActivateAsync"/> to finish.
    /// </summary>
    /// <param name="email">Sender email address.</param>
    /// <param name="name">Sender display name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(string email, string name, CancellationToken cancellationToken = default);

    /// <summary>Removes a sender address.</summary>
    /// <param name="email">Sender email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Emails a new activation code to a sender address. SendPulse allows one code per 15 minutes.
    /// </summary>
    /// <param name="email">Sender email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RequestActivationCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Activates a sender address with the code SendPulse emailed to it.</summary>
    /// <param name="email">Sender email address.</param>
    /// <param name="code">Activation code from the email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ActivateAsync(string email, string code, CancellationToken cancellationToken = default);
}
