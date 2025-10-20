using System.Text;
using System.Text.Json;
using SendPulse.Client.Common.Exceptions;
using SendPulse.Client.Templates.Entities;

namespace SendPulse.Client.Templates;

/// <summary>
/// Service for working with email templates
/// </summary>
public class TemplateService : ITemplateService
{
    private const string DefaultSingleEndpoint = "template";
    private const string DefaultPluralEndpoint = "templates";
    private readonly SendPulseClient _client;

    /// <summary>
    /// Service for working with email templates
    /// </summary>
    public TemplateService(SendPulseClient client, string singleEndpoint = DefaultSingleEndpoint, string pluralEndpoint = DefaultPluralEndpoint)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        SingleEndpoint = singleEndpoint;
        PluralEndpoint = pluralEndpoint;
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };


    /// <summary>
    /// Gets all email templates
    /// </summary>
    /// <param name="owner">Filter templates by owner (e.g., "me", "sendpulse"). If null, returns all templates.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of email templates</returns>
    public async Task<List<Template>> GetAsync(string? owner = null, CancellationToken cancellationToken = default)
    {
        var endpoint = PluralEndpoint;

        // Add owner filter if specified
        if (!string.IsNullOrWhiteSpace(owner))
        {
            endpoint = $"{PluralEndpoint}/?owner={owner}";
        }

        var response = await _client.GetAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new SendPulseException(
                $"Failed to get templates. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<Template>>(content, JsonOptions);

        return result ?? [];
    }

    /// <summary>
    /// Gets a specific email template by ID
    /// </summary>
    /// <param name="templateId">Template ID (GUID string)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email template</returns>
    public async Task<Template?> GetByIdAsync(string templateId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"{SingleEndpoint}/{templateId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new SendPulseException(
                $"Failed to get template. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Template>(content, JsonOptions);

        return result;
    }

    /// <summary>
    /// Creates a new email template
    /// </summary>
    /// <param name="request">Template data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created template response with ID</returns>
    public async Task<CreateTemplateResponse> CreateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Template name is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.BodyBase64))
            throw new ArgumentException("Template body is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.LanguageString))
            throw new ArgumentException("Template language is required", nameof(request));

        var json = JsonSerializer.Serialize(request, JsonOptions);

        // Debug: print the JSON being sent
        System.Diagnostics.Debug.WriteLine($"JSON being sent: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(SingleEndpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            // Debug: print full error details
            System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Error: {errorContent}");
            System.Diagnostics.Debug.WriteLine($"Request URL: {response.RequestMessage?.RequestUri}");
            System.Diagnostics.Debug.WriteLine($"Request Method: {response.RequestMessage?.Method}");

            throw new SendPulseException(
                $"Failed to create template. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateTemplateResponse>(responseContent, JsonOptions);

        return result ?? throw new SendPulseException("Invalid response from API when creating template");
    }

    /// <summary>
    /// Updates an existing email template
    /// </summary>
    /// <param name="templateId">Template ID as string</param>
    /// <param name="request">Updated template data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update response</returns>
    public async Task<UpdateTemplateResponse> EditAsync(string templateId, UpdateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID is required", nameof(templateId));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.BodyBase64))
            throw new ArgumentException("Template body is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.LanguageString))
            throw new ArgumentException("Template language is required", nameof(request));

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{SingleEndpoint}/edit/{templateId}", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new SendPulseException($"Failed to update template. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateTemplateResponse>(responseContent, JsonOptions);

        return result ?? throw new SendPulseException("Invalid response from API when updating template");
    }

    /// <summary>
    /// Updates an existing email template
    /// </summary>
    /// <param name="realId">Real ID (numeric)</param>
    /// <param name="request">Updated template data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update response</returns>
    public async Task<UpdateTemplateResponse> EditAsync(int realId, UpdateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (realId <= 0) throw new ArgumentException("Template ID is required", nameof(realId));
        return await EditAsync(realId.ToString(), request, cancellationToken);
    }

    private string SingleEndpoint { get; }
    
    private string PluralEndpoint { get; }
}