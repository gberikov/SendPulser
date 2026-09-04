namespace SendPulser.Templates;

/// <summary>
/// Email templates stored in the account.
/// </summary>
/// <remarks>
/// Wraps <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>.
/// </remarks>
public interface ITemplateService
{
    /// <summary>Lists templates.</summary>
    /// <param name="owner">
    /// Filters by owner: <c>me</c> returns the account templates, <c>sendpulse</c> the system ones.
    /// When omitted, the system templates are returned.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching templates.</returns>
    Task<IReadOnlyList<Template>> GetAllAsync(string? owner = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a single template.</summary>
    /// <param name="templateId">Template ID, either the string ID or the numeric one.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The template.</returns>
    /// <exception cref="SendPulserApiException">The template does not exist.</exception>
    Task<Template> GetAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>Gets a template by its URL friendly name.</summary>
    /// <param name="slug">Value of <see cref="Template.NameSlug"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The template.</returns>
    /// <exception cref="SendPulserApiException">The template does not exist.</exception>
    Task<Template> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Creates a template.</summary>
    /// <param name="request">Template name, body and language.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the new template.</returns>
    Task<CreateTemplateResult> CreateAsync(
        CreateTemplateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Edits a template.</summary>
    /// <param name="templateId">Template ID, either the string ID or the numeric one.</param>
    /// <param name="request">New body and language. The language must match the one used at creation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(
        string templateId,
        UpdateTemplateRequest request,
        CancellationToken cancellationToken = default);
}
