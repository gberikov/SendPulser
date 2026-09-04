using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Balance;

/// <summary>
/// Account balance in one currency.
/// </summary>
public sealed class AccountBalance
{
    /// <summary>Currency code, for example <c>USD</c>.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>Balance in that currency.</summary>
    [JsonPropertyName("balance_currency")]
    public decimal Amount { get; set; }
}

/// <summary>
/// Balance and plan details of every service the account uses.
/// </summary>
public sealed class BalanceDetails
{
    /// <summary>Money on the account.</summary>
    [JsonPropertyName("balance")]
    public MoneyBalance? Balance { get; set; }

    /// <summary>Bulk email plan, when the service is used.</summary>
    [JsonPropertyName("email")]
    public EmailPlan? Email { get; set; }

    /// <summary>SMTP plan, when the service is used.</summary>
    [JsonPropertyName("smtp")]
    public SmtpPlan? Smtp { get; set; }

    /// <summary>Web push plan, when the service is used.</summary>
    [JsonPropertyName("push")]
    public PushPlan? Push { get; set; }
}

/// <summary>
/// Main and bonus balance of the account.
/// </summary>
public sealed class MoneyBalance
{
    /// <summary>Main balance.</summary>
    [JsonPropertyName("main")]
    public decimal Main { get; set; }

    /// <summary>Bonus balance.</summary>
    [JsonPropertyName("bonus")]
    public decimal Bonus { get; set; }

    /// <summary>Currency code.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

/// <summary>
/// Bulk email plan of the account.
/// </summary>
public sealed class EmailPlan
{
    /// <summary>Plan name.</summary>
    [JsonPropertyName("tariff_name")]
    public string? Name { get; set; }

    /// <summary>When the plan ends, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("finished_time")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? EndsAt { get; set; }

    /// <summary>Emails left in the current period.</summary>
    [JsonPropertyName("emails_left")]
    public long EmailsLeft { get; set; }

    /// <summary>Maximum number of subscribers on the plan.</summary>
    [JsonPropertyName("maximum_subscribers")]
    public long MaximumSubscribers { get; set; }

    /// <summary>Current number of subscribers.</summary>
    [JsonPropertyName("current_subscribers")]
    public long CurrentSubscribers { get; set; }
}

/// <summary>
/// SMTP plan of the account.
/// </summary>
public sealed class SmtpPlan
{
    /// <summary>Plan name.</summary>
    [JsonPropertyName("tariff_name")]
    public string? Name { get; set; }

    /// <summary>When the plan ends, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("end_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? EndsAt { get; set; }

    /// <summary>Whether the plan renews automatically.</summary>
    [JsonPropertyName("auto_renew")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool AutoRenew { get; set; }

    /// <summary>Emails included in the plan.</summary>
    [JsonPropertyName("email_qty")]
    public long EmailQuota { get; set; }

    /// <summary>Emails left in the current period.</summary>
    [JsonPropertyName("email_qty_left")]
    public long EmailsLeft { get; set; }

    /// <summary>Maximum emails per hour.</summary>
    [JsonPropertyName("qty_per_hour")]
    public long EmailsPerHour { get; set; }

    /// <summary>Traffic included in the plan, as SendPulse formats it, for example <c>1000 Mb</c>.</summary>
    [JsonPropertyName("traffic_limit")]
    public string? TrafficLimit { get; set; }

    /// <summary>Traffic left, as SendPulse formats it.</summary>
    [JsonPropertyName("traffic_limit_left")]
    public string? TrafficLeft { get; set; }
}

/// <summary>
/// Web push plan of the account.
/// </summary>
public sealed class PushPlan
{
    /// <summary>Plan name.</summary>
    [JsonPropertyName("tariff_name")]
    public string? Name { get; set; }

    /// <summary>When the plan ends.</summary>
    [JsonPropertyName("end_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? EndsAt { get; set; }

    /// <summary>Whether the plan renews automatically.</summary>
    [JsonPropertyName("auto_renew")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool AutoRenew { get; set; }
}
