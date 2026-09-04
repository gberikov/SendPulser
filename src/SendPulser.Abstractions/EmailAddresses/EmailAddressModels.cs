using System.Text.Json.Serialization;
using SendPulser.AddressBooks;
using SendPulser.Json;

namespace SendPulser.EmailAddresses;

/// <summary>
/// The presence of an email address in one mailing list.
/// </summary>
public sealed class EmailAddressMembership
{
    /// <summary>Mailing list ID.</summary>
    [JsonPropertyName("book_id")]
    public int AddressBookId { get; set; }

    /// <summary>Email address. Absent from the batch endpoint, which keys its result by address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>Status code of the address in that list, see <see cref="ContactStatus"/>.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Human readable status, as returned by SendPulse.</summary>
    [JsonPropertyName("status_explain")]
    public string? StatusExplanation { get; set; }

    /// <summary>Variables of the contact in that list, empty when none are set.</summary>
    [JsonPropertyName("variables")]
    public IReadOnlyList<ContactVariable> Variables { get; set; } = [];
}

/// <summary>
/// When and how an email address was added to a mailing list.
/// </summary>
public sealed class EmailAddressListEntry
{
    /// <summary>Mailing list ID.</summary>
    [JsonPropertyName("list_id")]
    public int AddressBookId { get; set; }

    /// <summary>Mailing list name.</summary>
    [JsonPropertyName("list_name")]
    public string? AddressBookName { get; set; }

    /// <summary>Date the address was added, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("add_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? AddedAt { get; set; }

    /// <summary>How the address was collected, for example <c>panel</c>, <c>form</c> or <c>api</c>.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

/// <summary>
/// A mailing list referenced from the statistics of an email address.
/// </summary>
[JsonConverter(typeof(AddressBookReferenceConverter))]
public sealed class AddressBookReference
{
    /// <summary>Mailing list ID.</summary>
    public int Id { get; set; }

    /// <summary>Mailing list name.</summary>
    public string? Name { get; set; }
}

/// <summary>
/// Engagement counters of an email address across every campaign it received.
/// </summary>
public sealed class EmailAddressCounters
{
    /// <summary>Number of emails sent to the address.</summary>
    [JsonPropertyName("sent")]
    public int Sent { get; set; }

    /// <summary>Number of opens.</summary>
    [JsonPropertyName("open")]
    public int Opened { get; set; }

    /// <summary>Number of link clicks.</summary>
    [JsonPropertyName("link")]
    public int Clicked { get; set; }
}

/// <summary>
/// Statistics of a single email address, as returned by the single address endpoint.
/// </summary>
public sealed class EmailAddressStatistics
{
    /// <summary>Engagement counters.</summary>
    [JsonPropertyName("statistic")]
    public EmailAddressCounters Counters { get; set; } = new();

    /// <summary>Whether the address is on the account blacklist.</summary>
    [JsonPropertyName("blacklist")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsBlacklisted { get; set; }

    /// <summary>Mailing lists the address belongs to.</summary>
    [JsonPropertyName("addressbooks")]
    public IReadOnlyList<AddressBookReference> AddressBooks { get; set; } = [];
}

/// <summary>
/// Statistics of one email address, as returned by the batch endpoint. The batch endpoint flattens the
/// counters and spells the mailing list key differently, hence the separate type.
/// </summary>
public sealed class EmailAddressBatchStatistics
{
    /// <summary>Number of emails sent to the address.</summary>
    [JsonPropertyName("sent")]
    public int Sent { get; set; }

    /// <summary>Number of opens.</summary>
    [JsonPropertyName("open")]
    public int Opened { get; set; }

    /// <summary>Number of link clicks.</summary>
    [JsonPropertyName("link")]
    public int Clicked { get; set; }

    /// <summary>Whether the address is on the account blacklist.</summary>
    [JsonPropertyName("blacklist")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsBlacklisted { get; set; }

    /// <summary>Mailing lists the address belongs to. The misspelled key matches the SendPulse payload.</summary>
    [JsonPropertyName("adressbooks")]
    public IReadOnlyList<AddressBookReference> AddressBooks { get; set; } = [];
}
