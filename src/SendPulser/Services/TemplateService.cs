using SendPulser.Internal;
using SendPulser.Templates;

namespace SendPulser.Services;

internal sealed class TemplateService(SendPulserApi api) : ITemplateService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<Template>> GetAllAsync(
        string? owner = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(("owner", owner));

        return await _api
            .GetAsync("templates" + query, SendPulserJsonContext.Default.ListTemplate, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Template> GetAsync(string templateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        return await _api.GetAsync(
                $"template/{Uri.EscapeDataString(templateId)}",
                SendPulserJsonContext.Default.Template,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<CreateTemplateResult> CreateAsync(
        CreateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.CreateTemplateRequest);

        return await _api.SendAsync(
                HttpMethod.Post,
                "template",
                content,
                SendPulserJsonContext.Default.CreateTemplateResult,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        string templateId,
        UpdateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.UpdateTemplateRequest);

        // Editing is a POST to template/edit/{id}, not a PUT to template/{id}.
        await _api.SendAsync(
                HttpMethod.Post,
                $"template/edit/{Uri.EscapeDataString(templateId)}",
                content,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
