namespace SendPulser.Campaigns;

/// <summary>
/// Bulk email campaigns.
/// </summary>
/// <remarks>
/// Wraps <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>.
/// SendPulse accepts at most four campaigns per hour.
/// </remarks>
public interface ICampaignService
{
    /// <summary>Lists campaigns.</summary>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="order">Sort order, <c>asc</c> or <c>desc</c>.</param>
    /// <param name="statuses">Restricts the result to campaigns in these statuses, see <see cref="CampaignStatus"/>.</param>
    /// <param name="scheduled">When <see langword="true"/>, includes scheduled campaigns.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The campaigns of the account.</returns>
    Task<IReadOnlyList<Campaign>> GetAllAsync(
        int? limit = null,
        int? offset = null,
        string? order = null,
        IReadOnlyList<int>? statuses = null,
        bool? scheduled = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a campaign together with its per status statistics.</summary>
    /// <param name="campaignId">Campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The campaign.</returns>
    /// <exception cref="SendPulserApiException">The campaign does not exist.</exception>
    Task<CampaignInfo> GetAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Gets the number of opens per country.</summary>
    /// <param name="campaignId">Campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Opens keyed by ISO country code.</returns>
    Task<IReadOnlyDictionary<string, int>> GetCountryStatisticsAsync(
        int campaignId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the number of clicks per link.</summary>
    /// <param name="campaignId">Campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Clicks per link.</returns>
    Task<IReadOnlyList<CampaignReferral>> GetReferralStatisticsAsync(
        int campaignId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the delivery status of one recipient of a campaign.</summary>
    /// <param name="campaignId">Campaign ID.</param>
    /// <param name="email">Recipient address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The status.</returns>
    Task<CampaignRecipient> GetRecipientAsync(
        int campaignId,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a campaign. Unless <see cref="CreateCampaignRequest.Type"/> is <c>draft</c> or
    /// <see cref="CreateCampaignRequest.SendDate"/> is set, this starts sending immediately.
    /// </summary>
    /// <param name="request">Sender, subject, body or template, and recipients.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ID and initial status of the campaign.</returns>
    Task<CreateCampaignResult> CreateAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Edits a scheduled campaign.</summary>
    /// <param name="campaignId">Campaign ID.</param>
    /// <param name="request">New sender, subject, body and schedule.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(
        int campaignId,
        UpdateCampaignRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Cancels a campaign.</summary>
    /// <param name="campaignId">Campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CancelAsync(int campaignId, CancellationToken cancellationToken = default);
}
