namespace SendPulser.Tags;

/// <summary>
/// Tags of the email service and their assignment to contacts. Available on the Pro plan and above.
/// </summary>
/// <remarks>
/// Wraps the <c>/tags</c> endpoints of
/// <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>.
/// Every write is queued by SendPulse and acknowledged with a <see cref="TagOperationResult"/>; a
/// queued request whose <see cref="TagOperationResult.Success"/> is <see langword="false"/> is raised
/// as a <see cref="SendPulserApiException"/>.
/// </remarks>
public interface ITagService
{
    /// <summary>Lists the tags of the account.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tags.</returns>
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a tag.</summary>
    /// <param name="name">Tag name.</param>
    /// <param name="color">Tag color as a hex triplet, for example <c>#f0f4f6</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The queue acknowledgement.</returns>
    Task<TagOperationResult> CreateAsync(string name, string color, CancellationToken cancellationToken = default);

    /// <summary>Renames or recolors a tag.</summary>
    /// <param name="tagId">Tag ID.</param>
    /// <param name="name">New name.</param>
    /// <param name="color">New color.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The queue acknowledgement.</returns>
    Task<TagOperationResult> UpdateAsync(int tagId, string name, string color, CancellationToken cancellationToken = default);

    /// <summary>Deletes a tag.</summary>
    /// <param name="tagId">Tag ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The queue acknowledgement.</returns>
    Task<TagOperationResult> DeleteAsync(int tagId, CancellationToken cancellationToken = default);

    /// <summary>Attaches tags to an email address.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="tagIds">IDs of existing tags.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The queue acknowledgement.</returns>
    Task<TagOperationResult> AssignToEmailAsync(string email, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default);

    /// <summary>Attaches tags to a phone number.</summary>
    /// <param name="phone">Phone number.</param>
    /// <param name="tagIds">IDs of existing tags.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The queue acknowledgement.</returns>
    Task<TagOperationResult> AssignToPhoneAsync(string phone, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default);

    /// <summary>Detaches tags from an email address.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="tagIds">IDs of the tags to detach.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The queue acknowledgement.</returns>
    Task<TagOperationResult> UnassignFromEmailAsync(string email, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default);

    /// <summary>Detaches tags from a phone number.</summary>
    /// <param name="phone">Phone number.</param>
    /// <param name="tagIds">IDs of the tags to detach.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The queue acknowledgement.</returns>
    Task<TagOperationResult> UnassignFromPhoneAsync(string phone, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default);
}
