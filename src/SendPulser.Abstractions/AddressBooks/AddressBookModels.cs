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

    /// <summary>Date the mailing list was created, in the time zone of the SendPulse account.</summary>
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
/// A contact stored in a mailing list, as returned by the contact listing and search endpoints.
/// </summary>
public sealed class Contact
{
    /// <summary>Email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Status code of the address, see <see cref="ContactStatus"/>.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Human readable status, as returned by SendPulse.</summary>
    [JsonPropertyName("status_explain")]
    public string? StatusExplanation { get; set; }

    /// <summary>Phone number, when one was stored for the contact.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Contact variables by name. SendPulse types values per mailing list (string, number or date); every
    /// value is exposed as text, numbers in invariant culture and dates as <c>yyyy-MM-dd</c>.
    /// <see langword="null"/> when the endpoint does not return variables.
    /// </summary>
    [JsonPropertyName("variables")]
    [JsonConverter(typeof(VariableMapConverter))]
    public Dictionary<string, string?>? Variables { get; set; }
}

/// <summary>
/// A variable of a contact together with its declared type, as returned by the single contact endpoints.
/// </summary>
public sealed class ContactVariable
{
    /// <summary>Variable name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Variable type, one of <c>string</c>, <c>number</c> or <c>date</c>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Variable value as text.</summary>
    [JsonPropertyName("value")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Value { get; set; }
}

/// <summary>
/// A contact read from a specific mailing list, including its typed variables.
/// </summary>
public sealed class ContactDetails
{
    /// <summary>Email address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Mailing list ID.</summary>
    [JsonPropertyName("abook_id")]
    public int AddressBookId { get; set; }

    /// <summary>Phone number, empty when none is stored.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>Status code of the address, see <see cref="ContactStatus"/>.</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>Human readable status, as returned by SendPulse.</summary>
    [JsonPropertyName("status_explain")]
    public string? StatusExplanation { get; set; }

    /// <summary>Variables of the contact, empty when none are set.</summary>
    [JsonPropertyName("variables")]
    public IReadOnlyList<ContactVariable> Variables { get; set; } = [];
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
    /// Variable values. Use the system variable <c>Phone</c> to store a phone number; dates must be
    /// formatted as <c>yyyy-MM-dd</c>.
    /// </summary>
    [JsonPropertyName("variables")]
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// A variable value to store on an existing contact.
/// </summary>
public sealed class VariableUpdate
{
    /// <summary>Creates an empty update.</summary>
    public VariableUpdate()
    {
    }

    /// <summary>Creates an update.</summary>
    /// <param name="name">Variable name.</param>
    /// <param name="value">New value; dates as <c>yyyy-MM-dd</c>, numbers in invariant culture.</param>
    public VariableUpdate(string name, string value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>Variable name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>New value.</summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
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

/// <summary>
/// A campaign that was sent to a mailing list.
/// </summary>
public sealed class AddressBookCampaign
{
    /// <summary>Campaign ID.</summary>
    [JsonPropertyName("task_id")]
    public int Id { get; set; }

    /// <summary>Campaign name.</summary>
    [JsonPropertyName("task_name")]
    public string? Name { get; set; }

    /// <summary>Campaign status code, see <see cref="Campaigns.CampaignStatus"/>.</summary>
    [JsonPropertyName("task_status")]
    public int Status { get; set; }
}

/// <summary>
/// Estimated cost of sending one campaign to a mailing list.
/// </summary>
public sealed class CampaignCost
{
    /// <summary>Currency of the estimate.</summary>
    [JsonPropertyName("cur")]
    public string? Currency { get; set; }

    /// <summary>Total number of addresses the campaign would go to.</summary>
    [JsonPropertyName("sent_emails_qty")]
    public int EmailCount { get; set; }

    /// <summary>Price of the addresses above the plan limit.</summary>
    [JsonPropertyName("overdraftAllEmailsPrice")]
    public decimal OverdraftPrice { get; set; }

    /// <summary>Number of addresses charged from the account balance.</summary>
    [JsonPropertyName("addressesDeltaFromBalance")]
    public int AddressesFromBalance { get; set; }

    /// <summary>Number of addresses covered by the plan.</summary>
    [JsonPropertyName("addressesDeltaFromTariff")]
    public int AddressesFromPlan { get; set; }

    /// <summary>Maximum number of addresses per campaign on the current plan.</summary>
    [JsonPropertyName("max_emails_per_task")]
    public int MaxEmailsPerCampaign { get; set; }

    /// <summary>Whether the balance covers the campaign.</summary>
    [JsonPropertyName("result")]
    public bool IsAffordable { get; set; }
}
