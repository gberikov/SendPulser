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
    /// <summary>Lists campaigns, newest first.</summary>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="offset">Index of the first record to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The campaigns of the account.</returns>
    Task<IReadOnlyList<Campaign>> GetAllAsync(
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a campaign together with its per status statistics.</summary>
    /// <param name="campaignId">Campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The campaign.</returns>
    /// <exception cref="SendPulserApiException">The campaign does not exist.</exception>
    Task<CampaignInfo> GetAsync(int campaignId, CancellationToken cancellationToken = default);

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
