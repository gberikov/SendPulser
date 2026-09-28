using System.Text;
using SendPulser.Blacklist;
using SendPulser.Internal;

namespace SendPulser.Services;

internal sealed class BlacklistService(SendPulserApi api) : IBlacklistService
{
    // GET /blacklist answers 100 addresses per call and pages only through ?offset= (?limit= breaks the
    // response shape, ?page= is ignored). Undocumented; checked on a live account on 2026-09-28.
    private const int PageSize = 100;

    // ponytail: stops silently at 100 000 addresses; raise it if an account ever holds more.
    private const int MaxPages = 1000;

    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = new List<string>();
        List<string>? previous = null;

        for (var page = 0; page < MaxPages; page++)
        {
            var batch = await _api.GetAsync(
                    "blacklist" + SendPulserApi.BuildQuery(("offset", SendPulserApi.Number(page * PageSize))),
                    SendPulserJsonContext.Default.ListString,
                    cancellationToken)
                .ConfigureAwait(false);

            // A server that ignores the offset would hand back the same page forever.
            if (previous is not null && batch.SequenceEqual(previous))
            {
                break;
            }

            all.AddRange(batch);
            if (batch.Count < PageSize)
            {
                break;
            }

            previous = batch;
        }

        return all;
    }

    public async Task AddAsync(
        IReadOnlyList<string> emails,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new BlacklistRequest { Emails = Encode(emails), Comment = comment },
            SendPulserJsonContext.Default.BlacklistRequest);

        await _api.SendAsync(HttpMethod.Post, "blacklist", content, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(IReadOnlyList<string> emails, CancellationToken cancellationToken = default)
    {
        using var content = SendPulserApi.Json(
            new BlacklistRequest { Emails = Encode(emails) },
            SendPulserJsonContext.Default.BlacklistRequest);

        await _api.SendAsync(HttpMethod.Delete, "blacklist", content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// SendPulse wants the addresses as one comma separated string, Base64 encoded.
    /// </summary>
    internal static string Encode(IReadOnlyList<string> emails)
    {
        ArgumentNullException.ThrowIfNull(emails);
        if (emails.Count == 0)
        {
            throw new ArgumentException("At least one email address is required.", nameof(emails));
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(',', emails)));
    }
}
