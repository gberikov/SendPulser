using System.Text.Json.Serialization;

namespace SendPulser.Tags;

/// <summary>
/// A tag that can be attached to contacts. Tags require the Pro plan or above.
/// </summary>
public sealed class Tag
{
    /// <summary>Tag ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Tag name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Tag color as a hex triplet, for example <c>#f0f4f6</c>.</summary>
    [JsonPropertyName("color")]
    public string? Color { get; set; }
}

/// <summary>
/// Acknowledgement of a tag operation. SendPulse queues tag changes rather than applying them
/// synchronously, so the result names the queue entry instead of the changed tag.
/// </summary>
public sealed class TagOperationResult
{
    /// <summary>Whether the request was queued.</summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>Machine readable outcome, for example <c>request_sended_to_queue</c>.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Human readable outcome.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>ID of the queue entry.</summary>
    [JsonPropertyName("queue_id")]
    public string? QueueId { get; set; }
}
