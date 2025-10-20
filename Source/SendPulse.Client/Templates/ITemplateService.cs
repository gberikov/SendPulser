using SendPulse.Client.Templates.Entities;

namespace SendPulse.Client.Templates;

/// <summary>
/// Interface for working with email templates
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Gets all email templates
    /// </summary>
    /// <param name="owner">Filter templates by owner (e.g., "me", "sendpulse"). If null, returns all templates.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of email templates</returns>
    Task<List<Template>> GetAsync(string? owner = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific email template by ID
    /// </summary>
    /// <param name="templateId">Template ID (GUID string)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email template</returns>
    Task<Template?> GetByIdAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new email template
    /// </summary>
    /// <param name="request">Template data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created template response with ID</returns>
    Task<CreateTemplateResponse> CreateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing email template
    /// </summary>
    /// <param name="templateId">Template ID as string</param>
    /// <param name="request">Updated template data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update response</returns>
    Task<UpdateTemplateResponse> EditAsync(string templateId, UpdateTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing email template
    /// </summary>
    /// <param name="realId">Real ID (numeric)</param>
    /// <param name="request">Updated template data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update response</returns>
    Task<UpdateTemplateResponse> EditAsync(int realId, UpdateTemplateRequest request, CancellationToken cancellationToken = default);
}

