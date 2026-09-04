namespace SendPulser.Webhooks;

/// <summary>
/// Webhook registrations of the account.
/// </summary>
/// <remarks>
/// Wraps the <c>/v2/email-service/webhook</c> endpoints. Receiving the events these registrations
/// produce is the job of the <c>SendPulser.AspNetCore</c> package.
/// </remarks>
public interface IWebhookService
{
    /// <summary>Lists the registered webhooks, one row per subscribed event.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registered webhooks.</returns>
    Task<IReadOnlyList<Webhook>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a single webhook registration.</summary>
    /// <param name="webhookId">Webhook ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The webhook.</returns>
    /// <exception cref="SendPulserApiException">The webhook does not exist.</exception>
    Task<Webhook> GetAsync(int webhookId, CancellationToken cancellationToken = default);

    /// <summary>Subscribes a URL to one or more events.</summary>
    /// <param name="url">HTTPS URL events are posted to.</param>
    /// <param name="actions">
    /// Events to subscribe to, for example <c>delivered</c>, <c>open</c> or <c>unsubscribe</c>.
    /// SendPulse does not publish the full list of valid values.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created registrations, one per event.</returns>
    Task<IReadOnlyList<Webhook>> CreateAsync(
        string url,
        IReadOnlyList<string> actions,
        CancellationToken cancellationToken = default);

    /// <summary>Changes the URL of a registration.</summary>
    /// <param name="webhookId">Webhook ID.</param>
    /// <param name="url">New HTTPS URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(int webhookId, string url, CancellationToken cancellationToken = default);

    /// <summary>Deletes a registration.</summary>
    /// <param name="webhookId">Webhook ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int webhookId, CancellationToken cancellationToken = default);
}
