using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Campaigns;

/// <summary>
/// An email campaign as returned by the campaign list.
/// </summary>
public sealed class Campaign
{
    /// <summary>Campaign ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Campaign name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Campaign status code, see <see cref="CampaignStatus"/>.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Whether the campaign is an SMS campaign.</summary>
    [JsonPropertyName("is_sms")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsSms { get; set; }

    /// <summary>Whether the campaign is a Viber campaign.</summary>
    [JsonPropertyName("is_viber")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsViber { get; set; }

    /// <summary>Date the campaign was or will be sent, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("send_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SendDate { get; set; }

    /// <summary>Total number of recipient addresses.</summary>
    [JsonPropertyName("all_email_qty")]
    public int AllEmailCount { get; set; }

    /// <summary>Number of emails charged against the plan.</summary>
    [JsonPropertyName("tariff_email_qty")]
    public int TariffEmailCount { get; set; }

    /// <summary>Number of emails paid from the balance on top of the plan.</summary>
    [JsonPropertyName("paid_email_qty")]
    public int PaidEmailCount { get; set; }

    /// <summary>Price per email above the plan limit.</summary>
    [JsonPropertyName("overdraft_price")]
    public decimal? OverdraftPrice { get; set; }

    /// <summary>Campaign price.</summary>
    [JsonPropertyName("company_price")]
    public decimal? Price { get; set; }

    /// <summary>Currency of the prices.</summary>
    [JsonPropertyName("overdraft_currency")]
    public string? Currency { get; set; }

    /// <summary>Sender and subject details.</summary>
    [JsonPropertyName("message")]
    public CampaignMessage? Message { get; set; }

    /// <summary>Aggregated delivery counters.</summary>
    [JsonPropertyName("statistics")]
    public CampaignCounters? Statistics { get; set; }
}

/// <summary>
/// Sender, subject and body of a campaign.
/// </summary>
public sealed class CampaignMessage
{
    /// <summary>Sender name.</summary>
    [JsonPropertyName("sender_name")]
    public string? SenderName { get; set; }

    /// <summary>Sender email address.</summary>
    [JsonPropertyName("sender_email")]
    public string? SenderEmail { get; set; }

    /// <summary>Email subject.</summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    /// <summary>HTML body. Only present when a single campaign is requested.</summary>
    [JsonPropertyName("body")]
    public string? Body { get; set; }

    /// <summary>Preheader text.</summary>
    [JsonPropertyName("preheader")]
    public string? Preheader { get; set; }

    /// <summary>Attachments, as SendPulse describes them.</summary>
    [JsonPropertyName("attachments")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Attachments { get; set; }

    /// <summary>Mailing list the campaign was sent to.</summary>
    [JsonPropertyName("list_id")]
    public int ListId { get; set; }
}

/// <summary>
/// Delivery counters returned with the campaign list.
/// </summary>
public sealed class CampaignCounters
{
    /// <summary>Number of sent emails.</summary>
    [JsonPropertyName("sent")]
    public int Sent { get; set; }

    /// <summary>Number of delivered emails.</summary>
    [JsonPropertyName("delivered")]
    public int Delivered { get; set; }

    /// <summary>Number of opens.</summary>
    [JsonPropertyName("opening")]
    public int Opened { get; set; }

    /// <summary>Number of link clicks.</summary>
    [JsonPropertyName("link_redirected")]
    public int Clicked { get; set; }

    /// <summary>Number of unsubscribes.</summary>
    [JsonPropertyName("unsubscribe")]
    public int Unsubscribed { get; set; }

    /// <summary>Number of errors.</summary>
    [JsonPropertyName("error")]
    public int Errors { get; set; }
}

/// <summary>
/// Detailed campaign information, including per status statistics.
/// </summary>
public sealed class CampaignInfo
{
    /// <summary>Campaign ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Campaign name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Campaign status code, see <see cref="CampaignStatus"/>.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Whether the campaign is an SMS campaign.</summary>
    [JsonPropertyName("is_sms")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsSms { get; set; }

    /// <summary>Whether the campaign is a Viber campaign.</summary>
    [JsonPropertyName("is_viber")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsViber { get; set; }

    /// <summary>Sender, subject and body.</summary>
    [JsonPropertyName("message")]
    public CampaignMessage? Message { get; set; }

    /// <summary>Which tracking features are enabled.</summary>
    [JsonPropertyName("external_stat")]
    public CampaignTracking? Tracking { get; set; }

    /// <summary>Date the campaign was or will be sent, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("send_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SendDate { get; set; }

    /// <summary>Total number of recipient addresses.</summary>
    [JsonPropertyName("all_email_qty")]
    public int AllEmailCount { get; set; }

    /// <summary>Number of emails charged against the plan.</summary>
    [JsonPropertyName("tariff_email_qty")]
    public int TariffEmailCount { get; set; }

    /// <summary>Number of emails paid from the balance on top of the plan.</summary>
    [JsonPropertyName("paid_email_qty")]
    public int PaidEmailCount { get; set; }

    /// <summary>Price per email above the plan limit.</summary>
    [JsonPropertyName("overdraft_price")]
    public decimal? OverdraftPrice { get; set; }

    /// <summary>Campaign price.</summary>
    [JsonPropertyName("company_price")]
    public decimal? Price { get; set; }

    /// <summary>Currency of the prices.</summary>
    [JsonPropertyName("overdraft_currency")]
    public string? Currency { get; set; }

    /// <summary>Public archive link to the campaign.</summary>
    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    /// <summary>Per status statistics.</summary>
    [JsonPropertyName("statistics")]
    public CampaignStatistics? Statistics { get; set; }
}

/// <summary>
/// Tracking features of a campaign.
/// </summary>
public sealed class CampaignTracking
{
    /// <summary>Whether opens are tracked.</summary>
    [JsonPropertyName("check_open_email")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool TracksOpens { get; set; }

    /// <summary>Whether link clicks are tracked.</summary>
    [JsonPropertyName("check_redirect_link")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool TracksClicks { get; set; }
}

/// <summary>
/// Per status statistics of a campaign.
/// </summary>
public sealed class CampaignStatistics
{
    /// <summary>Counters per delivery status code, see <see cref="DeliveryStatus"/>.</summary>
    [JsonPropertyName("general")]
    public IReadOnlyList<CampaignStatusCount> General { get; set; } = [];

    /// <summary>Click counters per link.</summary>
    [JsonPropertyName("clicks")]
    public IReadOnlyList<CampaignLinkClicks> Clicks { get; set; } = [];
}

/// <summary>
/// Number of recipients in one delivery status.
/// </summary>
public sealed class CampaignStatusCount
{
    /// <summary>Delivery status code, see <see cref="DeliveryStatus"/>.</summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>Number of recipients in this status.</summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>Human readable status, as returned by SendPulse.</summary>
    [JsonPropertyName("explain")]
    public string? Explanation { get; set; }
}

/// <summary>
/// Click counter for a single link of a campaign.
/// </summary>
public sealed class CampaignLinkClicks
{
    /// <summary>Link URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Number of clicks.</summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

/// <summary>
/// Click counter for a single link, as returned by the referral statistics.
/// </summary>
public sealed class CampaignReferral
{
    /// <summary>Link URL.</summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>Number of clicks.</summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

/// <summary>
/// Delivery status of one recipient of a campaign.
/// </summary>
public sealed class CampaignRecipient
{
    /// <summary>When the email was sent, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("sent_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SentAt { get; set; }

    /// <summary>Global status code, see <see cref="DeliveryStatus"/>.</summary>
    [JsonPropertyName("global_status")]
    public int GlobalStatus { get; set; }

    /// <summary>Human readable global status.</summary>
    [JsonPropertyName("global_status_explain")]
    public string? GlobalStatusExplanation { get; set; }

    /// <summary>Detailed status code, see <see cref="DeliveryStatus"/>.</summary>
    [JsonPropertyName("detail_status")]
    public int DetailStatus { get; set; }

    /// <summary>Human readable detailed status.</summary>
    [JsonPropertyName("detail_status_explain")]
    public string? DetailStatusExplanation { get; set; }
}

/// <summary>
/// Payload for creating a campaign. SendPulse allows at most four campaigns per hour.
/// </summary>
public sealed class CreateCampaignRequest
{
    /// <summary>Sender name.</summary>
    [JsonPropertyName("sender_name")]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>Sender email address, must be activated in the SendPulse account.</summary>
    [JsonPropertyName("sender_email")]
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Email subject.</summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>Campaign name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// HTML body. Pass plain markup: it is Base64 encoded on the wire. Either this or
    /// <see cref="TemplateId"/> is required.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonConverter(typeof(Base64BodyConverter))]
    public string? Body { get; set; }

    /// <summary>
    /// ID of a template stored in the service, either the string ID or the numeric one.
    /// Either this or <see cref="Body"/> is required.
    /// </summary>
    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// Mailing lists to send to, up to ten. Test and segmented campaigns accept exactly one.
    /// Serialized as a bare number when a single list is given, which is what SendPulse expects.
    /// </summary>
    [JsonPropertyName("list_id")]
    [JsonConverter(typeof(SingleOrArrayConverter))]
    public IReadOnlyList<int>? ListIds { get; set; }

    /// <summary>Segment to send to instead of a whole mailing list.</summary>
    [JsonPropertyName("segment_id")]
    public int? SegmentId { get; set; }

    /// <summary>Sends a test email to the sender address instead of the mailing list.</summary>
    [JsonPropertyName("is_test")]
    public bool? IsTest { get; set; }

    /// <summary>
    /// Schedules the campaign. SendPulse interprets the value in the time zone of the account, not in
    /// UTC, and rejects dates in the past with error code 799.
    /// </summary>
    [JsonPropertyName("send_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SendDate { get; set; }

    /// <summary>Includes contacts added to the list after the campaign was scheduled.</summary>
    [JsonPropertyName("use_dynamic_list")]
    public bool? UseDynamicList { get; set; }

    /// <summary>Set to <c>draft</c> to create the campaign without sending it.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Text attachments, keyed by file name.</summary>
    [JsonPropertyName("attachments")]
    public Dictionary<string, string>? Attachments { get; set; }
}

/// <summary>
/// Result of creating a campaign.
/// </summary>
public sealed class CreateCampaignResult
{
    /// <summary>Campaign ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Initial status, see <see cref="CampaignStatus"/>: 13 while addresses are being copied, 26 for a draft.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Number of recipients.</summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>Number of emails charged against the plan.</summary>
    [JsonPropertyName("tariff_email_qty")]
    public int TariffEmailCount { get; set; }

    /// <summary>Price per email above the plan limit.</summary>
    [JsonPropertyName("overdraft_price")]
    public decimal? OverdraftPrice { get; set; }

    /// <summary>Currency of the overdraft price. The misspelling matches the SendPulse payload.</summary>
    [JsonPropertyName("ovedraft_currency")]
    public string? OverdraftCurrency { get; set; }
}

/// <summary>
/// Payload for editing a scheduled campaign.
/// </summary>
public sealed class UpdateCampaignRequest
{
    /// <summary>Campaign name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Sender name.</summary>
    [JsonPropertyName("sender_name")]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>Sender email address.</summary>
    [JsonPropertyName("sender_email")]
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Email subject.</summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>HTML body, Base64 encoded on the wire.</summary>
    [JsonPropertyName("body")]
    [JsonConverter(typeof(Base64BodyConverter))]
    public string? Body { get; set; }

    /// <summary>ID of a template stored in the service.</summary>
    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }

    /// <summary>New scheduled date, interpreted in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("send_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SendDate { get; set; }
}
