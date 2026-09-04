using System.Net;
using SendPulser.AddressBooks;
using SendPulser.Internal;

namespace SendPulser.Services;

internal sealed class AddressBookService(SendPulserApi api) : IAddressBookService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<AddressBook>> GetAllAsync(
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)));

        return await _api
            .GetAsync("addressbooks" + query, SendPulserJsonContext.Default.ListAddressBook, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AddressBook> GetAsync(int addressBookId, CancellationToken cancellationToken = default)
    {
        // SendPulse answers this endpoint with a single element array rather than an object.
        var books = await _api.GetAsync(
                Path(addressBookId),
                SendPulserJsonContext.Default.ListAddressBook,
                cancellationToken)
            .ConfigureAwait(false);

        return books.Count > 0
            ? books[0]
            : throw new SendPulserApiException(
                $"SendPulse has no mailing list with ID {addressBookId}.",
                HttpStatusCode.NotFound,
                errorCode: null,
                responseBody: null);
    }

    public async Task<int> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new CreateAddressBookRequest { BookName = name },
            SendPulserJsonContext.Default.CreateAddressBookRequest);

        var created = await _api.SendAsync(
                HttpMethod.Post,
                "addressbooks",
                content,
                SendPulserJsonContext.Default.IdResponse,
                cancellationToken)
            .ConfigureAwait(false);

        return created.Id;
    }

    public async Task RenameAsync(int addressBookId, string name, CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new RenameAddressBookRequest { Name = name },
            SendPulserJsonContext.Default.RenameAddressBookRequest);

        await _api.SendAsync(HttpMethod.Put, Path(addressBookId), content, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(int addressBookId, CancellationToken cancellationToken = default) =>
        _api.SendAsync(HttpMethod.Delete, Path(addressBookId), content: null, cancellationToken);

    public async Task<IReadOnlyList<AddressBookVariable>> GetVariablesAsync(
        int addressBookId,
        CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                Path(addressBookId) + "/variables",
                SendPulserJsonContext.Default.ListAddressBookVariable,
                cancellationToken)
            .ConfigureAwait(false);

    public Task<CampaignCost> GetCampaignCostAsync(int addressBookId, CancellationToken cancellationToken = default) =>
        _api.GetAsync(Path(addressBookId) + "/cost", SendPulserJsonContext.Default.CampaignCost, cancellationToken);

    public async Task<IReadOnlyList<AddressBookCampaign>> GetCampaignsAsync(
        int addressBookId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)));

        return await _api.GetAsync(
                Path(addressBookId) + "/campaigns" + query,
                SendPulserJsonContext.Default.ListAddressBookCampaign,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Contact>> GetContactsAsync(
        int addressBookId,
        int? limit = null,
        int? offset = null,
        bool? active = null,
        bool? notActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)),
            ("active", SendPulserApi.Flag(active)),
            ("not_active", SendPulserApi.Flag(notActive)));

        return await _api.GetAsync(
                Path(addressBookId) + "/emails" + query,
                SendPulserJsonContext.Default.ListContact,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<ContactDetails> GetContactAsync(
        int addressBookId,
        string email,
        CancellationToken cancellationToken = default) =>
        _api.GetAsync(
            Path(addressBookId) + "/emails/" + SendPulserApi.Segment(email),
            SendPulserJsonContext.Default.ContactDetails,
            cancellationToken);

    public async Task<IReadOnlyList<Contact>> FindContactsByVariableAsync(
        int addressBookId,
        string variableName,
        string value,
        CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                $"{Path(addressBookId)}/variables/{SendPulserApi.Segment(variableName)}/{SendPulserApi.Segment(value)}",
                SendPulserJsonContext.Default.ListContact,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<int> GetContactCountAsync(int addressBookId, CancellationToken cancellationToken = default)
    {
        var total = await _api.GetAsync(
                Path(addressBookId) + "/emails/total",
                SendPulserJsonContext.Default.TotalResponse,
                cancellationToken)
            .ConfigureAwait(false);

        return total.Total;
    }

    public Task AddContactsAsync(
        int addressBookId,
        IReadOnlyList<NewContact> contacts,
        IReadOnlyList<int>? tagIds = null,
        CancellationToken cancellationToken = default) =>
        PostContactsAsync(
            addressBookId,
            new AddContactsRequest { Emails = contacts, Tags = tagIds },
            cancellationToken);

    public Task AddContactsWithConfirmationAsync(
        int addressBookId,
        IReadOnlyList<NewContact> contacts,
        DoubleOptInSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return PostContactsAsync(
            addressBookId,
            new AddContactsRequest
            {
                Emails = contacts,
                Confirmation = "force",
                SenderEmail = settings.SenderEmail,
                MessageLanguage = settings.MessageLanguage,
                TemplateId = settings.TemplateId,
            },
            cancellationToken);
    }

    public async Task UpdateVariablesAsync(
        int addressBookId,
        string email,
        IReadOnlyList<VariableUpdate> variables,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        using var content = SendPulserApi.Json(
            new UpdateVariablesRequest { Email = email, Variables = variables },
            SendPulserJsonContext.Default.UpdateVariablesRequest);

        await _api.SendAsync(HttpMethod.Post, Path(addressBookId) + "/emails/variable", content, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task SetPhoneAsync(
        int addressBookId,
        string email,
        string phone,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        using var content = SendPulserApi.Json(
            new SetPhoneRequest { Email = email, Phone = phone },
            SendPulserJsonContext.Default.SetPhoneRequest);

        await _api.SendAsync(HttpMethod.Put, Path(addressBookId) + "/phone", content, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UnsubscribeContactsAsync(
        int addressBookId,
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new EmailListRequest { Emails = emails },
            SendPulserJsonContext.Default.EmailListRequest);

        await _api.SendAsync(HttpMethod.Post, Path(addressBookId) + "/emails/unsubscribe", content, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task DeleteContactsAsync(
        int addressBookId,
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new EmailListRequest { Emails = emails },
            SendPulserJsonContext.Default.EmailListRequest);

        await _api.SendAsync(HttpMethod.Delete, Path(addressBookId) + "/emails", content, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string Path(int addressBookId) => "addressbooks/" + SendPulserApi.Number(addressBookId);

    private async Task PostContactsAsync(
        int addressBookId,
        AddContactsRequest request,
        CancellationToken cancellationToken)
    {
        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.AddContactsRequest);

        await _api.SendAsync(HttpMethod.Post, Path(addressBookId) + "/emails", content, cancellationToken)
            .ConfigureAwait(false);
    }
}
