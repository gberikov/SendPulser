namespace SendPulser.AddressBooks;

/// <summary>
/// Mailing lists (address books) and the contacts stored in them.
/// </summary>
/// <remarks>
/// Wraps <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>.
/// Listing methods map <c>limit</c> and <c>offset</c> straight through; SendPulse returns at most
/// 100 contacts per call. Operations that look up an address across every mailing list live on
/// <see cref="EmailAddresses.IEmailAddressService"/>.
/// </remarks>
public interface IAddressBookService
{
    /// <summary>Lists mailing lists.</summary>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mailing lists of the account.</returns>
    Task<IReadOnlyList<AddressBook>> GetAllAsync(
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a single mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The mailing list.</returns>
    /// <exception cref="SendPulserApiException">The mailing list does not exist.</exception>
    Task<AddressBook> GetAsync(int addressBookId, CancellationToken cancellationToken = default);

    /// <summary>Creates a mailing list.</summary>
    /// <param name="name">Mailing list name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ID of the new mailing list.</returns>
    Task<int> CreateAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Renames a mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="name">New name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RenameAsync(int addressBookId, string name, CancellationToken cancellationToken = default);

    /// <summary>Deletes a mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int addressBookId, CancellationToken cancellationToken = default);

    /// <summary>Lists the variables defined on a mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The variables of the mailing list.</returns>
    Task<IReadOnlyList<AddressBookVariable>> GetVariablesAsync(
        int addressBookId,
        CancellationToken cancellationToken = default);

    /// <summary>Estimates what sending one campaign to the mailing list costs.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The estimate.</returns>
    Task<CampaignCost> GetCampaignCostAsync(int addressBookId, CancellationToken cancellationToken = default);

    /// <summary>Lists the campaigns that were sent to a mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The campaigns.</returns>
    Task<IReadOnlyList<AddressBookCampaign>> GetCampaignsAsync(
        int addressBookId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the contacts of a mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="limit">Maximum number of records to return; SendPulse caps this at 100.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="active">When <see langword="true"/>, returns only contacts in the New and Active statuses.</param>
    /// <param name="notActive">When <see langword="true"/>, returns only inactive contacts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contacts of the mailing list.</returns>
    Task<IReadOnlyList<Contact>> GetContactsAsync(
        int addressBookId,
        int? limit = null,
        int? offset = null,
        bool? active = null,
        bool? notActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one contact of a mailing list, including its typed variables.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="email">Email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contact.</returns>
    /// <exception cref="SendPulserApiException">The contact is not in the mailing list.</exception>
    Task<ContactDetails> GetContactAsync(
        int addressBookId,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>Finds the contacts of a mailing list whose variable has a given value.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="variableName">Variable name.</param>
    /// <param name="value">Value to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching contacts, without their variables.</returns>
    Task<IReadOnlyList<Contact>> FindContactsByVariableAsync(
        int addressBookId,
        string variableName,
        string value,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the total number of contacts in a mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of contacts.</returns>
    Task<int> GetContactCountAsync(int addressBookId, CancellationToken cancellationToken = default);

    /// <summary>Adds contacts to a mailing list without asking them to confirm.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="contacts">Contacts to add.</param>
    /// <param name="tagIds">IDs of existing tags to assign. Requires the Pro plan or above.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddContactsAsync(
        int addressBookId,
        IReadOnlyList<NewContact> contacts,
        IReadOnlyList<int>? tagIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>Adds contacts to a mailing list and asks them to confirm the subscription.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="contacts">Contacts to add.</param>
    /// <param name="settings">Sender and language of the confirmation email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddContactsWithConfirmationAsync(
        int addressBookId,
        IReadOnlyList<NewContact> contacts,
        DoubleOptInSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces variable values of one contact. SendPulse accepts a single contact per call.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="email">Email address of the contact.</param>
    /// <param name="variables">Variables to set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateVariablesAsync(
        int addressBookId,
        string email,
        IReadOnlyList<VariableUpdate> variables,
        CancellationToken cancellationToken = default);

    /// <summary>Sets or replaces the phone number of a contact.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="email">Email address of the contact.</param>
    /// <param name="phone">Phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetPhoneAsync(
        int addressBookId,
        string email,
        string phone,
        CancellationToken cancellationToken = default);

    /// <summary>Marks contacts as unsubscribed in a mailing list without deleting them.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="emails">Addresses to unsubscribe.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UnsubscribeContactsAsync(
        int addressBookId,
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default);

    /// <summary>Removes contacts from a mailing list.</summary>
    /// <param name="addressBookId">Mailing list ID.</param>
    /// <param name="emails">Addresses to remove; SendPulse accepts up to 100 per call.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteContactsAsync(
        int addressBookId,
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default);
}
