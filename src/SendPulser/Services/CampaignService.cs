using SendPulser.Campaigns;
using SendPulser.Internal;

namespace SendPulser.Services;

internal sealed class CampaignService(SendPulserApi api) : ICampaignService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<Campaign>> GetAllAsync(
        int? limit = null,
        int? offset = null,
        string? order = null,
        IReadOnlyList<int>? statuses = null,
        bool? scheduled = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<(string Name, string? Value)>
        {
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)),
            ("order", order),
            ("planed", SendPulserApi.Flag(scheduled)),
        };

        foreach (var status in statuses ?? [])
        {
            parameters.Add(("status[]", SendPulserApi.Number(status)));
        }

        return await _api
            .GetAsync("campaigns" + SendPulserApi.BuildQuery(parameters), SendPulserJsonContext.Default.ListCampaign, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<CampaignInfo> GetAsync(int campaignId, CancellationToken cancellationToken = default) =>
        _api.GetAsync(Path(campaignId), SendPulserJsonContext.Default.CampaignInfo, cancellationToken);

    public async Task<IReadOnlyDictionary<string, int>> GetCountryStatisticsAsync(
        int campaignId,
        CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                Path(campaignId) + "/countries",
                SendPulserJsonContext.Default.DictionaryStringInt32,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<CampaignReferral>> GetReferralStatisticsAsync(
        int campaignId,
        CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                Path(campaignId) + "/referrals",
                SendPulserJsonContext.Default.ListCampaignReferral,
                cancellationToken)
            .ConfigureAwait(false);

    public Task<CampaignRecipient> GetRecipientAsync(
        int campaignId,
        string email,
        CancellationToken cancellationToken = default) =>
        _api.GetAsync(
            Path(campaignId) + "/email/" + SendPulserApi.Segment(email),
            SendPulserJsonContext.Default.CampaignRecipient,
            cancellationToken);

    public async Task<CreateCampaignResult> CreateAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.CreateCampaignRequest);

        return await _api.SendAsync(
                HttpMethod.Post,
                "campaigns",
                content,
                SendPulserJsonContext.Default.CreateCampaignResult,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        int campaignId,
        UpdateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.UpdateCampaignRequest);

        await _api.SendAsync(HttpMethod.Patch, Path(campaignId), content, cancellationToken).ConfigureAwait(false);
    }

    public Task CancelAsync(int campaignId, CancellationToken cancellationToken = default) =>
        _api.SendAsync(HttpMethod.Delete, Path(campaignId), content: null, cancellationToken);

    private static string Path(int campaignId) => "campaigns/" + SendPulserApi.Number(campaignId);
}
