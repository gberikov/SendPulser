using System.Net;
using System.Text.Json.Serialization.Metadata;
using SendPulser.Internal;
using SendPulser.Tags;

namespace SendPulser.Services;

internal sealed class TagService(SendPulserApi api) : ITagService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _api.GetAsync("tags", SendPulserJsonContext.Default.TagListResponse, cancellationToken)
            .ConfigureAwait(false);

        return response.Tags;
    }

    public Task<TagOperationResult> CreateAsync(string name, string color, CancellationToken cancellationToken = default) =>
        QueueAsync(
            HttpMethod.Post,
            "tags",
            new TagRequest { Name = name, Color = color },
            SendPulserJsonContext.Default.TagRequest,
            cancellationToken);

    public Task<TagOperationResult> UpdateAsync(int tagId, string name, string color, CancellationToken cancellationToken = default) =>
        QueueAsync(
            HttpMethod.Put,
            "tags/" + SendPulserApi.Number(tagId),
            new TagRequest { Name = name, Color = color },
            SendPulserJsonContext.Default.TagRequest,
            cancellationToken);

    public async Task<TagOperationResult> DeleteAsync(int tagId, CancellationToken cancellationToken = default)
    {
        var result = await _api.SendAsync(
                HttpMethod.Delete,
                "tags/" + SendPulserApi.Number(tagId),
                content: null,
                SendPulserJsonContext.Default.TagOperationResult,
                cancellationToken)
            .ConfigureAwait(false);

        return EnsureQueued(result, HttpMethod.Delete, "tags/" + SendPulserApi.Number(tagId));
    }

    public Task<TagOperationResult> AssignToEmailAsync(string email, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default) =>
        QueueAsync(
            HttpMethod.Post,
            "tags/pin/email",
            new TagEmailRequest { Email = email, Tags = tagIds },
            SendPulserJsonContext.Default.TagEmailRequest,
            cancellationToken);

    public Task<TagOperationResult> AssignToPhoneAsync(string phone, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default) =>
        QueueAsync(
            HttpMethod.Post,
            "tags/pin/phone",
            new TagPhoneRequest { Phone = phone, Tags = tagIds },
            SendPulserJsonContext.Default.TagPhoneRequest,
            cancellationToken);

    public Task<TagOperationResult> UnassignFromEmailAsync(string email, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default) =>
        QueueAsync(
            HttpMethod.Post,
            "tags/unpin/email",
            new TagEmailRequest { Email = email, Tags = tagIds },
            SendPulserJsonContext.Default.TagEmailRequest,
            cancellationToken);

    public Task<TagOperationResult> UnassignFromPhoneAsync(string phone, IReadOnlyList<int> tagIds, CancellationToken cancellationToken = default) =>
        QueueAsync(
            HttpMethod.Post,
            "tags/unpin/phone",
            new TagPhoneRequest { Phone = phone, Tags = tagIds },
            SendPulserJsonContext.Default.TagPhoneRequest,
            cancellationToken);

    private async Task<TagOperationResult> QueueAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken)
    {
        using var content = SendPulserApi.Json(request, requestTypeInfo);

        var result = await _api.SendAsync(
                method,
                path,
                content,
                SendPulserJsonContext.Default.TagOperationResult,
                cancellationToken)
            .ConfigureAwait(false);

        return EnsureQueued(result, method, path);
    }

    private static TagOperationResult EnsureQueued(TagOperationResult result, HttpMethod method, string path) =>
        result.Success
            ? result
            : throw new SendPulserApiException(
                $"SendPulse did not queue {method} {path}: {result.Description ?? result.Code ?? "no reason given"}",
                HttpStatusCode.OK,
                errorCode: null,
                responseBody: null);
}
