using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SendPulser.Internal;
using SendPulser.Webhooks;

namespace SendPulser.Services;

internal sealed class WebhookService(SendPulserApi api) : IWebhookService
{
    private const string BasePath = "v2/email-service/webhook";

    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<Webhook>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var body = await _api.SendRawAsync(HttpMethod.Get, BasePath, content: null, cancellationToken)
            .ConfigureAwait(false);

        return ReadList(body);
    }

    public async Task<Webhook> GetAsync(int webhookId, CancellationToken cancellationToken = default)
    {
        var body = await _api.SendRawAsync(
                HttpMethod.Get,
                $"{BasePath}/{webhookId.ToString(CultureInfo.InvariantCulture)}",
                content: null,
                cancellationToken)
            .ConfigureAwait(false);

        var webhooks = ReadList(body);
        return webhooks.Count > 0
            ? webhooks[0]
            : throw new SendPulserApiException(
                $"SendPulse has no webhook with ID {webhookId}.",
                HttpStatusCode.NotFound,
                errorCode: null,
                body);
    }

    public async Task<IReadOnlyList<Webhook>> CreateAsync(
        string url,
        IReadOnlyList<string> actions,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new CreateWebhookRequest { Url = url, Actions = actions },
            SendPulserJsonContext.Default.CreateWebhookRequest);

        var body = await _api.SendRawAsync(HttpMethod.Post, BasePath, content, cancellationToken)
            .ConfigureAwait(false);

        return ReadList(body);
    }

    public async Task UpdateAsync(int webhookId, string url, CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new UpdateWebhookRequest { Url = url },
            SendPulserJsonContext.Default.UpdateWebhookRequest);

        await _api.SendRawAsync(
                HttpMethod.Put,
                $"{BasePath}/{webhookId.ToString(CultureInfo.InvariantCulture)}",
                content,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(int webhookId, CancellationToken cancellationToken = default) =>
        await _api.SendRawAsync(
                HttpMethod.Delete,
                $"{BasePath}/{webhookId.ToString(CultureInfo.InvariantCulture)}",
                content: null,
                cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Reads a webhook payload. The v2 endpoints wrap their result in one or two <c>data</c> envelopes and
    /// return a single row either bare or inside an array, so both shapes are flattened here.
    /// </summary>
    private static List<Webhook> ReadList(string body)
    {
        using var document = JsonDocument.Parse(body);

        var element = document.RootElement;
        while (element.ValueKind is JsonValueKind.Object && element.TryGetProperty("data", out var inner))
        {
            element = inner;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Array => Deserialize(element, SendPulserJsonContext.Default.ListWebhook) ?? [],
            JsonValueKind.Object => Deserialize(element, SendPulserJsonContext.Default.Webhook) is { } single
                ? [single]
                : [],
            _ => [],
        };
    }

    private static T? Deserialize<T>(JsonElement element, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            return element.Deserialize(typeInfo);
        }
        catch (JsonException exception)
        {
            throw new SendPulserApiException(
                "SendPulse returned a webhook payload that does not match the expected shape.",
                exception);
        }
    }
}
