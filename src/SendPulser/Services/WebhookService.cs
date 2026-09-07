using System.Net;
using System.Text.Json;
using SendPulser.Internal;
using SendPulser.Webhooks;

namespace SendPulser.Services;

internal sealed class WebhookService(SendPulserApi api) : IWebhookService
{
    private const string BasePath = "v2/email-service/webhook";

    private readonly SendPulserApi _api = api;

    public Task<IReadOnlyList<Webhook>> GetAllAsync(CancellationToken cancellationToken = default) =>
        ReadListAsync(HttpMethod.Get, BasePath, content: null, cancellationToken);

    public async Task<Webhook> GetAsync(int webhookId, CancellationToken cancellationToken = default)
    {
        var webhooks = await ReadListAsync(HttpMethod.Get, Path(webhookId), content: null, cancellationToken)
            .ConfigureAwait(false);

        return webhooks.Count > 0
            ? webhooks[0]
            : throw new SendPulserApiException(
                $"SendPulse has no webhook with ID {webhookId}.",
                HttpStatusCode.NotFound,
                errorCode: null,
                responseBody: null);
    }

    public async Task<IReadOnlyList<Webhook>> CreateAsync(
        string url,
        IReadOnlyList<string> actions,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(actions);

        var fields = new List<KeyValuePair<string, string>>
        {
            new("url", url),
        };
        fields.AddRange(actions.Select(action => new KeyValuePair<string, string>("actions[]", action)));
        using var content = new FormUrlEncodedContent(fields);

        return await ReadListAsync(HttpMethod.Post, BasePath, content, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(int webhookId, string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        using var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("url", url)]);

        await _api.SendAsync(HttpMethod.Put, Path(webhookId), content, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(int webhookId, CancellationToken cancellationToken = default) =>
        _api.SendAsync(HttpMethod.Delete, Path(webhookId), content: null, cancellationToken);

    private static string Path(int webhookId) => BasePath + "/" + SendPulserApi.Number(webhookId);

    /// <summary>
    /// Reads a webhook payload. The v2 endpoints wrap their result in one or two <c>data</c> envelopes and
    /// return a single row either bare or inside an array, so both shapes are flattened here.
    /// </summary>
    private async Task<IReadOnlyList<Webhook>> ReadListAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var body = await _api.SendRawAsync(method, path, content, cancellationToken).ConfigureAwait(false);
        using var document = SendPulserApi.ParseDocument(body);
        var payload = SendPulserApi.Unwrap(document.RootElement, body, method, path);

        try
        {
            return payload.ValueKind switch
            {
                JsonValueKind.Array => payload.Deserialize(SendPulserJsonContext.Default.ListWebhook) ?? [],
                JsonValueKind.Object => payload.Deserialize(SendPulserJsonContext.Default.Webhook) is { } single
                    ? [single]
                    : [],
                _ => [],
            };
        }
        catch (JsonException exception)
        {
            throw new SendPulserApiException(
                "SendPulse returned a webhook payload that does not match the expected shape.",
                exception);
        }
    }
}
