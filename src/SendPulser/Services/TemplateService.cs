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

    public Task<Template> GetAsync(string templateId, CancellationToken cancellationToken = default) =>
        _api.GetAsync(
            "template/" + SendPulserApi.Segment(templateId),
            SendPulserJsonContext.Default.Template,
            cancellationToken);

    public Task<Template> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _api.GetAsync(
            "template/slug/" + SendPulserApi.Segment(slug),
            SendPulserJsonContext.Default.Template,
            cancellationToken);

    public async Task<CreateTemplateResult> CreateAsync(
        CreateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.CreateTemplateRequest);

        return await _api.SendAsync(
                HttpMethod.Post,
                "template",
                content,
                SendPulserJsonContext.Default.CreateTemplateResult,
                cancellationToken,
                ensureAccepted: true)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        string templateId,
        UpdateTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var content = SendPulserApi.Json(request, SendPulserJsonContext.Default.UpdateTemplateRequest);

        // Editing is a POST to template/edit/{id}, not a PUT to template/{id}.
        await _api.SendAsync(
                HttpMethod.Post,
                "template/edit/" + SendPulserApi.Segment(templateId),
                content,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
