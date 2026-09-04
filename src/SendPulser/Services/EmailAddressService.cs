using SendPulser.EmailAddresses;
using SendPulser.Internal;

namespace SendPulser.Services;

internal sealed class EmailAddressService(SendPulserApi api) : IEmailAddressService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<EmailAddressMembership>> GetAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                "emails/" + SendPulserApi.Segment(email),
                SendPulserJsonContext.Default.ListEmailAddressMembership,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<EmailAddressListEntry>> GetDetailsAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                "emails/" + SendPulserApi.Segment(email) + "/details",
                SendPulserJsonContext.Default.ListEmailAddressListEntry,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<EmailAddressMembership>>> GetManyAsync(
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new EmailListRequest { Emails = emails },
            SendPulserJsonContext.Default.EmailListRequest);

        return await _api.SendAsync(
                HttpMethod.Post,
                "emails",
                content,
                SendPulserJsonContext.Default.DictionaryStringIReadOnlyListEmailAddressMembership,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<EmailAddressStatistics> GetStatisticsAsync(string email, CancellationToken cancellationToken = default) =>
        _api.GetAsync(
            "emails/" + SendPulserApi.Segment(email) + "/campaigns",
            SendPulserJsonContext.Default.EmailAddressStatistics,
            cancellationToken);

    public async Task<IReadOnlyDictionary<string, EmailAddressBatchStatistics>> GetStatisticsAsync(
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new EmailListRequest { Emails = emails },
            SendPulserJsonContext.Default.EmailListRequest);

        return await _api.SendAsync(
                HttpMethod.Post,
                "emails/campaigns",
                content,
                SendPulserJsonContext.Default.DictionaryStringEmailAddressBatchStatistics,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task DeleteFromAllListsAsync(string email, CancellationToken cancellationToken = default) =>
        _api.SendAsync(HttpMethod.Delete, "emails/" + SendPulserApi.Segment(email), content: null, cancellationToken);
}
