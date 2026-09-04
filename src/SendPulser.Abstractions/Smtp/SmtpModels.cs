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
    public bool Result { get; set; }

    /// <summary>ID of the sent message, used to look up its delivery status later.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// A message processed by the SMTP service.
/// </summary>
public sealed class SmtpEmail
{
    /// <summary>Message ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

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
    public string? SmtpAnswerSubcode { get; set; }

    /// <summary>Raw SMTP response text.</summary>
    [JsonPropertyName("smtp_answer_data")]
    public string? SmtpAnswerData { get; set; }

    /// <summary>Date the message was sent.</summary>
    [JsonPropertyName("send_date")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? SendDate { get; set; }
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
    public int UnsubscribedByLink { get; set; }

    /// <summary>Whether the contact was unsubscribed by the account owner.</summary>
    [JsonPropertyName("unsubscribe_by_user")]
    public int UnsubscribedByUser { get; set; }

    /// <summary>Whether the contact filed a spam complaint.</summary>
    [JsonPropertyName("spam_complaint")]
    public int SpamComplaint { get; set; }

    /// <summary>Date of the unsubscribe.</summary>
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
