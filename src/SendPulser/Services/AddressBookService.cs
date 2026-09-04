using System.Globalization;
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
                $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}",
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

        await _api.SendAsync(
                HttpMethod.Put,
                $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}",
                content,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task DeleteAsync(int addressBookId, CancellationToken cancellationToken = default) =>
        _api.SendAsync(
            HttpMethod.Delete,
            $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}",
            content: null,
            cancellationToken);

    public async Task<IReadOnlyList<AddressBookVariable>> GetVariablesAsync(
        int addressBookId,
        CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}/variables",
                SendPulserJsonContext.Default.ListAddressBookVariable,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Contact>> GetContactsAsync(
        int addressBookId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)));

        return await _api.GetAsync(
                $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}/emails" + query,
                SendPulserJsonContext.Default.ListContact,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> GetContactCountAsync(int addressBookId, CancellationToken cancellationToken = default)
    {
        var total = await _api.GetAsync(
                $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}/emails/total",
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

    public async Task DeleteContactsAsync(
        int addressBookId,
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new EmailListRequest { Emails = emails },
            SendPulserJsonContext.Default.EmailListRequest);

        await _api.SendAsync(
                HttpMethod.Delete,
                $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}/emails",
                content,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task PostContactsAsync(
        int addressBookId,
        AddContactsRequest request,
        CancellationToken cancellationToken)
    {
        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.AddContactsRequest);

        await _api.SendAsync(
                HttpMethod.Post,
                $"addressbooks/{addressBookId.ToString(CultureInfo.InvariantCulture)}/emails",
                content,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
