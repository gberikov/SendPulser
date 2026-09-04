using System.Net;

namespace SendPulser.AspNetCore;

/// <summary>
/// Protection settings for a webhook endpoint.
/// </summary>
/// <remarks>
/// SendPulse signs nothing: webhook payloads carry no HMAC, no shared secret header and no verifiable
/// origin. A secret embedded in the URL, optionally paired with an address allowlist, is therefore the
/// only authentication available. Serve the endpoint over HTTPS so the secret is not sent in the clear.
/// </remarks>
public sealed class SendPulserWebhookOptions
{
    /// <summary>
    /// Secret that must appear in the request. It is read from the <c>secret</c> route value, so the
    /// endpoint pattern is expected to contain <c>{secret}</c>, and falls back to a <c>secret</c> query
    /// string parameter. When left empty the endpoint accepts anonymous requests.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Addresses allowed to post to the endpoint. Empty means every address is allowed. SendPulse does
    /// not publish a stable list of sending addresses, so pin these only if you have verified them for
    /// your own account, and remember that behind a proxy this requires forwarded headers to be handled.
    /// </summary>
    public IList<IPAddress> AllowedAddresses { get; } = [];
}
