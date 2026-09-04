using SendPulser.Internal;
using SendPulser.Senders;

namespace SendPulser.Services;

internal sealed class SenderService(SendPulserApi api) : ISenderService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<Sender>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _api.GetAsync("senders", SendPulserJsonContext.Default.ListSender, cancellationToken)
            .ConfigureAwait(false);

    public async Task AddAsync(string email, string name, CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new SenderRequest { Email = email, Name = name },
            SendPulserJsonContext.Default.SenderRequest);

        await _api.SendAsync(HttpMethod.Post, "senders", content, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string email, CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new SenderRequest { Email = email },
            SendPulserJsonContext.Default.SenderRequest);

        await _api.SendAsync(HttpMethod.Delete, "senders", content, cancellationToken).ConfigureAwait(false);
    }

    public Task RequestActivationCodeAsync(string email, CancellationToken cancellationToken = default) =>
        _api.SendAsync(HttpMethod.Get, CodePath(email), content: null, cancellationToken);

    public async Task ActivateAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        using var content = SendPulserApi.Json(
            new ActivationCodeRequest { Code = code },
            SendPulserJsonContext.Default.ActivationCodeRequest);

        await _api.SendAsync(HttpMethod.Post, CodePath(email), content, cancellationToken).ConfigureAwait(false);
    }

    private static string CodePath(string email) => "senders/" + SendPulserApi.Segment(email) + "/code";
}
