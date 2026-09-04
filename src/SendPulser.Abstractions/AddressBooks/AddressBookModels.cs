using System.Text.Json;
using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.AddressBooks;

/// <summary>
/// A SendPulse mailing list (address book).
/// </summary>
public sealed class AddressBook
{
    /// <summary>Mailing list ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Mailing list name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Total number of email addresses.</summary>
    [JsonPropertyName("all_email_qty")]
    public int AllEmailCount { get; set; }

    /// <summary>Number of active email addresses.</summary>
    [JsonPropertyName("active_email_qty")]
    public int ActiveEmailCount { get; set; }

    /// <summary>Number of inactive email addresses.</summary>
    [JsonPropertyName("inactive_email_qty")]
    public int InactiveEmailCount { get; set; }

    /// <summary>Date the mailing list was created.</summary>
    [JsonPropertyName("creationdate")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? CreatedAt { get; set; }

    /// <summary>Status code, see <see cref="StatusExplanation"/> for the service supplied description.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Human readable status, as returned by SendPulse.</summary>
    [JsonPropertyName("status_explain")]
    public string? StatusExplanation { get; set; }
}

/// <summary>
/// A variable defined on a mailing list.
/// </summary>
public sealed class AddressBookVariable
{
    /// <summary>Variable name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Variable type, one of <c>string</c>, <c>number</c> or <c>date</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// A contact stored in a mailing list.
/// </summary>
public sealed class Contact
{
    /// <summary>Email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Status code of the address.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Human readable status, as returned by SendPulse.</summary>
    [JsonPropertyName("status_explain")]
    public string? StatusExplanation { get; set; }

    /// <summary>Phone number, when one was stored for the contact.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Contact variables. Values are exposed as <see cref="JsonElement"/> because SendPulse types them
    /// per mailing list: the same variable can arrive as a string, a number or a date.
    /// </summary>
    [JsonPropertyName("variables")]
    public Dictionary<string, JsonElement>? Variables { get; set; }
}

/// <summary>
/// A contact to add to a mailing list.
/// </summary>
public sealed class NewContact
{
    /// <summary>Creates an empty contact.</summary>
    public NewContact()
    {
    }

    /// <summary>Creates a contact for the given address.</summary>
    /// <param name="email">Email address.</param>
    public NewContact(string email) => Email = email;

    /// <summary>Email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Variable values. Use the system variable <c>Phone</c> to store a phone number.
    /// </summary>
    [JsonPropertyName("variables")]
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// Double opt-in settings for importing contacts.
/// </summary>
public sealed class DoubleOptInSettings
{
    /// <summary>Creates the settings.</summary>
    /// <param name="senderEmail">Sender address, must be activated in the SendPulse account.</param>
    /// <param name="messageLanguage">Confirmation email language: <c>en</c>, <c>ru</c>, <c>ua</c>, <c>tr</c>, <c>es</c> or <c>pt</c>.</param>
    public DoubleOptInSettings(string senderEmail, string messageLanguage)
    {
        SenderEmail = senderEmail;
        MessageLanguage = messageLanguage;
    }

    /// <summary>Sender address used for the confirmation email.</summary>
    public string SenderEmail { get; }

    /// <summary>Confirmation email language.</summary>
    public string MessageLanguage { get; }

    /// <summary>Optional ID of the confirmation email template.</summary>
    public string? TemplateId { get; set; }
}
