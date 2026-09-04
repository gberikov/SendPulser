namespace SendPulser.Blacklist;

/// <summary>
/// The account blacklist: addresses that never receive campaigns, whatever mailing list they are in.
/// </summary>
/// <remarks>
/// Wraps the <c>/blacklist</c> endpoints of
/// <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>.
/// SendPulse expects the addresses as one comma separated, Base64 encoded string; the encoding is done here.
/// </remarks>
public interface IBlacklistService
{
    /// <summary>Lists the blacklisted addresses.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The addresses.</returns>
    Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds addresses to the blacklist.</summary>
    /// <param name="emails">Addresses to add.</param>
    /// <param name="comment">Optional note stored with the entries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(IReadOnlyList<string> emails, string? comment = null, CancellationToken cancellationToken = default);

    /// <summary>Removes addresses from the blacklist.</summary>
    /// <param name="emails">Addresses to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(IReadOnlyList<string> emails, CancellationToken cancellationToken = default);
}
