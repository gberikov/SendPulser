using SendPulser.Internal;
using SendPulser.Smtp;

namespace SendPulser.Services;

internal sealed class SmtpService(SendPulserApi api) : ISmtpService
{
    private readonly SendPulserApi _api = api;

    public async Task<SendEmailResult> SendAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var content = SendPulserApi.Json(
            new SendEmailEnvelope { Email = request },
            SendPulserJsonContext.Default.SendEmailEnvelope);

        return await _api.SendAsync(
                HttpMethod.Post,
                "smtp/emails",
                content,
                SendPulserJsonContext.Default.SendEmailResult,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SmtpEmail>> GetEmailsAsync(
        int? limit = null,
        int? offset = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? sender = null,
        string? recipient = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)),
            ("from", SendPulserApi.Date(fromDate)),
            ("to", SendPulserApi.Date(toDate)),
            ("sender", sender),
            ("recipient", recipient));

        return await _api
            .GetAsync("smtp/emails" + query, SendPulserJsonContext.Default.ListSmtpEmail, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SmtpEmail> GetEmailAsync(string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        return await _api.GetAsync(
                $"smtp/emails/{Uri.EscapeDataString(messageId)}",
                SendPulserJsonContext.Default.SmtpEmail,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        var total = await _api
            .GetAsync("smtp/emails/total", SendPulserJsonContext.Default.TotalResponse, cancellationToken)
            .ConfigureAwait(false);

        return total.Total;
    }

    public async Task UnsubscribeAsync(
        IReadOnlyList<UnsubscribeRequest> requests,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new UnsubscribeListRequest { Emails = requests },
            SendPulserJsonContext.Default.UnsubscribeListRequest);

        await _api.SendAsync(HttpMethod.Post, "smtp/unsubscribe", content, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task RemoveFromUnsubscribeListAsync(
        IReadOnlyList<string> emails,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new EmailListRequest { Emails = emails },
            SendPulserJsonContext.Default.EmailListRequest);

        await _api.SendAsync(HttpMethod.Delete, "smtp/unsubscribe", content, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UnsubscribedContact>> GetUnsubscribedAsync(
        DateOnly? onDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = SendPulserApi.BuildQuery(
            ("date", SendPulserApi.Date(onDate)),
            ("limit", SendPulserApi.Number(limit)),
            ("offset", SendPulserApi.Number(offset)));

        return await _api.GetAsync(
                "smtp/unsubscribe" + query,
                SendPulserJsonContext.Default.ListUnsubscribedContact,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetSendersAsync(CancellationToken cancellationToken = default) =>
        await _api
            .GetAsync("smtp/senders", SendPulserJsonContext.Default.ListString, cancellationToken)
            .ConfigureAwait(false);
}
