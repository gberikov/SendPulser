using System.Text;
using SendPulser.Blacklist;
using SendPulser.Internal;

namespace SendPulser.Services;

internal sealed class BlacklistService(SendPulserApi api) : IBlacklistService
{
    private readonly SendPulserApi _api = api;

    public async Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _api.GetAsync("blacklist", SendPulserJsonContext.Default.ListString, cancellationToken)
            .ConfigureAwait(false);

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
