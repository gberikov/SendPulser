namespace SendPulser.EmailAddresses;

/// <summary>
/// Operations on an email address across every mailing list of the account.
/// </summary>
/// <remarks>
/// Wraps the <c>/emails</c> endpoints of
/// <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>.
/// Operations scoped to one mailing list live on <see cref="AddressBooks.IAddressBookService"/>.
/// </remarks>
public interface IEmailAddressService
{
    /// <summary>Lists every mailing list an address is in, with its status and variables in each.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>One entry per mailing list; empty when the address is unknown.</returns>
    Task<IReadOnlyList<EmailAddressMembership>> GetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Lists when and how an address was added to each mailing list.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>One entry per mailing list.</returns>
    Task<IReadOnlyList<EmailAddressListEntry>> GetDetailsAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>Looks up several addresses at once.</summary>
    /// <param name="emails">Email addresses.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Memberships keyed by address.</returns>
    Task<IReadOnlyDictionary<string, IReadOnlyList<EmailAddressMembership>>> GetManyAsync(
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the engagement statistics of an address and the mailing lists it belongs to.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The statistics.</returns>
    Task<EmailAddressStatistics> GetStatisticsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Gets the engagement statistics of several addresses at once.</summary>
    /// <param name="emails">Email addresses.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Statistics keyed by address.</returns>
    Task<IReadOnlyDictionary<string, EmailAddressBatchStatistics>> GetStatisticsAsync(
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default);

    /// <summary>Removes an address from every mailing list of the account.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteFromAllListsAsync(string email, CancellationToken cancellationToken = default);
}
