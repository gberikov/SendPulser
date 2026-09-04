using System.Globalization;
using SendPulser.Campaigns;
using SendPulser.Internal;

namespace SendPulser.Services;

internal sealed class CampaignService(SendPulserApi api) : ICampaignService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<Campaign>> GetAllAsync(
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)));

        return await _api
            .GetAsync("campaigns" + query, SendPulserJsonContext.Default.ListCampaign, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<CampaignInfo> GetAsync(int campaignId, CancellationToken cancellationToken = default) =>
        await _api.GetAsync(
                $"campaigns/{campaignId.ToString(CultureInfo.InvariantCulture)}",
                SendPulserJsonContext.Default.CampaignInfo,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<CreateCampaignResult> CreateAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
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
        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.UpdateCampaignRequest);

        await _api.SendAsync(
                HttpMethod.Patch,
                $"campaigns/{campaignId.ToString(CultureInfo.InvariantCulture)}",
                content,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task CancelAsync(int campaignId, CancellationToken cancellationToken = default) =>
        _api.SendAsync(
            HttpMethod.Delete,
            $"campaigns/{campaignId.ToString(CultureInfo.InvariantCulture)}",
            content: null,
            cancellationToken);
}
