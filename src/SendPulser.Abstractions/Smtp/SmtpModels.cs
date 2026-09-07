using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Smtp;

/// <summary>
/// A named email address.
/// </summary>
public sealed class EmailAddress
{
    /// <summary>Creates an empty address.</summary>
    public EmailAddress()
    {
    }

    /// <summary>Creates an address.</summary>
    /// <param name="email">Email address.</param>
    /// <param name="name">Display name.</param>
    public EmailAddress(string email, string? name = null)
    {
        Email = email;
        Name = name;
    }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Reference to a template stored in SendPulse, together with the variables it expects.
/// </summary>
public sealed class SmtpTemplateReference
{
    /// <summary>Template ID, either the string ID or the numeric one.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Variable values substituted into the template.</summary>
    [JsonPropertyName("variables")]
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// A transactional email to send through the SMTP service.
/// </summary>
public sealed class SendEmailRequest
{
    /// <summary>Email subject.</summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>Sender.</summary>
    [JsonPropertyName("from")]
    public EmailAddress From { get; set; } = new();

    /// <summary>Recipients.</summary>
    [JsonPropertyName("to")]
    public IReadOnlyList<EmailAddress> To { get; set; } = [];

    /// <summary>
    /// HTML body. Pass plain markup: it is Base64 encoded on the wire. Required unless
    /// <see cref="Template"/> is used.
    /// </summary>
    [JsonPropertyName("html")]
    [JsonConverter(typeof(Base64BodyConverter))]
    public string? Html { get; set; }

    /// <summary>Plain text body.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>Template to render instead of an inline body.</summary>
    [JsonPropertyName("template")]
    public SmtpTemplateReference? Template { get; set; }

    /// <summary>Generates the text part automatically when it was not supplied.</summary>
    [JsonPropertyName("auto_plain_text")]
    public bool? AutoPlainText { get; set; }

    /// <summary>Reply-To address.</summary>
    [JsonPropertyName("reply_to")]
    public EmailAddress? ReplyTo { get; set; }

    /// <summary>Visible copy recipients.</summary>
    [JsonPropertyName("cc")]
    public IReadOnlyList<EmailAddress>? Cc { get; set; }

    /// <summary>Hidden copy recipients.</summary>
    [JsonPropertyName("bcc")]
    public IReadOnlyList<EmailAddress>? Bcc { get; set; }

    /// <summary>Text attachments, keyed by file name.</summary>
    [JsonPropertyName("attachments")]
    public Dictionary<string, string>? Attachments { get; set; }

    /// <summary>Binary attachments, keyed by file name. Content is Base64 encoded on the wire.</summary>
    [JsonPropertyName("attachments_binary")]
    [JsonConverter(typeof(Base64AttachmentsConverter))]
    public Dictionary<string, byte[]>? BinaryAttachments { get; set; }
}

/// <summary>
/// Result of sending a transactional email.
/// </summary>
public sealed class SendEmailResult
{
    /// <summary>Whether SendPulse accepted the email.</summary>
    [JsonPropertyName("result")]
    [JsonRequired]
    public bool Result { get; set; }

    /// <summary>ID of the sent message, used to look up its delivery status later.</summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; set; }
}

/// <summary>
/// A message processed by the SMTP service.
/// </summary>
public sealed class SmtpEmail
{
    /// <summary>Message ID.</summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; set; }

    /// <summary>Sender address.</summary>
    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    /// <summary>Recipient address.</summary>
    [JsonPropertyName("recipient")]
    public string? Recipient { get; set; }

    /// <summary>Email subject.</summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    /// <summary>Total message size in bytes.</summary>
    [JsonPropertyName("total_size")]
    public long TotalSize { get; set; }

    /// <summary>IP address the message was submitted from.</summary>
    [JsonPropertyName("sender_ip")]
    public string? SenderIp { get; set; }

    /// <summary>IP address SendPulse delivered from.</summary>
    [JsonPropertyName("used_ip")]
    public string? UsedIp { get; set; }

    /// <summary>SMTP response code of the receiving server.</summary>
    [JsonPropertyName("smtp_answer_code")]
    public int? SmtpAnswerCode { get; set; }

    /// <summary>SMTP enhanced status code.</summary>
    [JsonPropertyName("smtp_answer_subcode")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? SmtpAnswerSubcode { get; set; }

    /// <summary>Raw SMTP response text.</summary>
    [JsonPropertyName("smtp_answer_data")]
    public string? SmtpAnswerData { get; set; }

    /// <summary>Date the message was sent, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("send_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SendDate { get; set; }

    /// <summary>Open and click tracking, when SendPulse recorded any.</summary>
    [JsonPropertyName("tracking")]
    public SmtpTracking? Tracking { get; set; }
}

/// <summary>
/// Open and click tracking of one transactional email.
/// </summary>
public sealed class SmtpTracking
{
    /// <summary>Number of clicks.</summary>
    [JsonPropertyName("click")]
    public int Clicks { get; set; }

    /// <summary>Number of opens.</summary>
    [JsonPropertyName("open")]
    public int Opens { get; set; }

    /// <summary>Each recorded click.</summary>
    [JsonPropertyName("link")]
    public IReadOnlyList<SmtpLinkClick> Links { get; set; } = [];

    /// <summary>Each recorded open.</summary>
    [JsonPropertyName("client_info")]
    public IReadOnlyList<SmtpClientInfo> Clients { get; set; } = [];
}

/// <summary>
/// One click on a link in a transactional email.
/// </summary>
public sealed class SmtpLinkClick
{
    /// <summary>Clicked URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Browser name and version.</summary>
    [JsonPropertyName("browser")]
    public string? Browser { get; set; }

    /// <summary>Operating system.</summary>
    [JsonPropertyName("os")]
    public string? OperatingSystem { get; set; }

    /// <summary>Screen resolution, for example <c>1920x1080</c>.</summary>
    [JsonPropertyName("screen_resolution")]
    public string? ScreenResolution { get; set; }

    /// <summary>IP address of the reader.</summary>
    [JsonPropertyName("ip")]
    public string? IpAddress { get; set; }

    /// <summary>Country of the reader, as SendPulse names it.</summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>When the click happened, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("action_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? ActionDate { get; set; }
}

/// <summary>
/// One open of a transactional email.
/// </summary>
public sealed class SmtpClientInfo
{
    /// <summary>Mail client or browser name and version.</summary>
    [JsonPropertyName("browser")]
    public string? Browser { get; set; }

    /// <summary>Operating system.</summary>
    [JsonPropertyName("os")]
    public string? OperatingSystem { get; set; }

    /// <summary>IP address of the reader.</summary>
    [JsonPropertyName("ip")]
    public string? IpAddress { get; set; }

    /// <summary>Country of the reader, as SendPulse names it.</summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>When the open happened, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("action_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? ActionDate { get; set; }
}

/// <summary>
/// One page of bounced transactional emails.
/// </summary>
public sealed class SmtpBouncePage
{
    /// <summary>Total number of bounces in the period.</summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>Number of bounces in this page.</summary>
    [JsonPropertyName("found")]
    public int Found { get; set; }

    /// <summary>Maximum page size SendPulse allows.</summary>
    [JsonPropertyName("request_limit")]
    public int RequestLimit { get; set; }

    /// <summary>The bounces.</summary>
    [JsonPropertyName("emails")]
    public IReadOnlyList<SmtpBounce> Bounces { get; set; } = [];
}

/// <summary>
/// A transactional email the receiving server rejected.
/// </summary>
public sealed class SmtpBounce
{
    /// <summary>Recipient address.</summary>
    [JsonPropertyName("email_to")]
    public string? Recipient { get; set; }

    /// <summary>Sender address.</summary>
    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    /// <summary>Email subject.</summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    /// <summary>When the email was sent, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("send_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SendDate { get; set; }

    /// <summary>SMTP response code of the receiving server.</summary>
    [JsonPropertyName("smtp_answer_code")]
    public int? SmtpAnswerCode { get; set; }

    /// <summary>SMTP enhanced status code.</summary>
    [JsonPropertyName("smtp_answer_subcode")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? SmtpAnswerSubcode { get; set; }

    /// <summary>Raw SMTP response text.</summary>
    [JsonPropertyName("smtp_answer_data")]
    public string? SmtpAnswerData { get; set; }
}

/// <summary>
/// A contact on the SMTP unsubscribe list.
/// </summary>
public sealed class UnsubscribedContact
{
    /// <summary>Email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Whether the contact unsubscribed through the link in an email.</summary>
    [JsonPropertyName("unsubscribe_by_link")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool UnsubscribedByLink { get; set; }

    /// <summary>Whether the contact was unsubscribed by the account owner.</summary>
    [JsonPropertyName("unsubscribe_by_user")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool UnsubscribedByUser { get; set; }

    /// <summary>Whether the contact filed a spam complaint.</summary>
    [JsonPropertyName("spam_complaint")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool SpamComplaint { get; set; }

    /// <summary>Date of the unsubscribe, in the time zone of the SendPulse account.</summary>
    [JsonPropertyName("date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? Date { get; set; }
}

/// <summary>
/// An address to add to the SMTP unsubscribe list.
/// </summary>
public sealed class UnsubscribeRequest
{
    /// <summary>Creates an empty request.</summary>
    public UnsubscribeRequest()
    {
    }

    /// <summary>Creates a request.</summary>
    /// <param name="email">Email address to unsubscribe.</param>
    /// <param name="comment">Reason kept alongside the record.</param>
    public UnsubscribeRequest(string email, string? comment = null)
    {
        Email = email;
        Comment = comment;
    }

    /// <summary>Email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Free form comment stored with the record.</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

/// <summary>
/// A domain registered for sending through the SMTP service.
/// </summary>
public sealed class SenderDomain
{
    /// <summary>Record ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>ID of the account that owns the domain.</summary>
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    /// <summary>Service type responsible for this domain. SendPulse currently returns <c>3</c>.</summary>
    [JsonPropertyName("service_type")]
    public int ServiceType { get; set; }

    /// <summary>Domain name.</summary>
    [JsonPropertyName("service_value")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>Domain expiry date, when applicable.</summary>
    [JsonPropertyName("expire_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Whether SendPulse automatically extends the free domain period.</summary>
    [JsonPropertyName("auto_free_prolong")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool AutoFreeProlong { get; set; }

    /// <summary>Currency associated with the domain record.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>Whether the domain is active: <c>1</c> active, <c>0</c> inactive.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Whether this is the default sending domain.</summary>
    [JsonPropertyName("is_default")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool IsDefault { get; set; }

    /// <summary>Certificate type: <c>0</c> none, <c>1</c> generated by SendPulse, <c>2</c> supplied by the account owner.</summary>
    [JsonPropertyName("ssl_type")]
    public int SslType { get; set; }

    /// <summary>Expiry of the certificate supplied by the account owner.</summary>
    [JsonPropertyName("ssl_expired")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SslExpiresAt { get; set; }

    /// <summary>SendPulse SSL certificate generation label.</summary>
    [JsonPropertyName("ssl_generated")]
    public int? SslGenerated { get; set; }

    /// <summary>DNS validation results.</summary>
    [JsonPropertyName("checks")]
    public SenderDomainChecks? Checks { get; set; }
}

/// <summary>
/// DNS validation of a sending domain.
/// </summary>
public sealed class SenderDomainChecks
{
    /// <summary>Whether the DKIM record is in place.</summary>
    [JsonPropertyName("check_dkim")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Dkim { get; set; }

    /// <summary>Whether the SPF record is in place.</summary>
    [JsonPropertyName("check_spf")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Spf { get; set; }

    /// <summary>Whether the DMARC record is in place.</summary>
    [JsonPropertyName("check_dmarc")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Dmarc { get; set; }

    /// <summary>Whether every check passed.</summary>
    [JsonPropertyName("all_checks")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool All { get; set; }

    /// <summary>The SPF TXT record SendPulse expects to find.</summary>
    [JsonPropertyName("spf_txt_needed")]
    public string? RequiredSpfRecord { get; set; }
}
