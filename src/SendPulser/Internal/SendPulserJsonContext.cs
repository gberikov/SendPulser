using System.Text.Json.Serialization;
using SendPulser.AddressBooks;
using SendPulser.Campaigns;
using SendPulser.Smtp;
using SendPulser.Templates;
using SendPulser.Webhooks;

namespace SendPulser.Internal;

/// <summary>
/// Source generated metadata for every payload the client serializes, so the library stays usable under
/// trimming and Native AOT. <see cref="JsonNumberHandling.AllowReadingFromString"/> is on because
/// SendPulse returns numeric fields as strings in about half of its endpoints.
/// </summary>
[JsonSourceGenerationOptions(
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TokenRequest))]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(ApiError))]
[JsonSerializable(typeof(ResultResponse))]
[JsonSerializable(typeof(IdResponse))]
[JsonSerializable(typeof(TotalResponse))]
[JsonSerializable(typeof(CreateAddressBookRequest))]
[JsonSerializable(typeof(RenameAddressBookRequest))]
[JsonSerializable(typeof(AddContactsRequest))]
[JsonSerializable(typeof(EmailListRequest))]
[JsonSerializable(typeof(UnsubscribeListRequest))]
[JsonSerializable(typeof(SendEmailEnvelope))]
[JsonSerializable(typeof(CreateWebhookRequest))]
[JsonSerializable(typeof(UpdateWebhookRequest))]
[JsonSerializable(typeof(AddressBook))]
[JsonSerializable(typeof(List<AddressBook>))]
[JsonSerializable(typeof(List<AddressBookVariable>))]
[JsonSerializable(typeof(List<Contact>))]
[JsonSerializable(typeof(Template))]
[JsonSerializable(typeof(TemplateCategory))]
[JsonSerializable(typeof(List<Template>))]
[JsonSerializable(typeof(CreateTemplateRequest))]
[JsonSerializable(typeof(CreateTemplateResult))]
[JsonSerializable(typeof(UpdateTemplateRequest))]
[JsonSerializable(typeof(Campaign))]
[JsonSerializable(typeof(List<Campaign>))]
[JsonSerializable(typeof(CampaignInfo))]
[JsonSerializable(typeof(CreateCampaignRequest))]
[JsonSerializable(typeof(CreateCampaignResult))]
[JsonSerializable(typeof(UpdateCampaignRequest))]
[JsonSerializable(typeof(SendEmailResult))]
[JsonSerializable(typeof(SmtpEmail))]
[JsonSerializable(typeof(List<SmtpEmail>))]
[JsonSerializable(typeof(List<UnsubscribedContact>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Webhook))]
[JsonSerializable(typeof(List<Webhook>))]
internal sealed partial class SendPulserJsonContext : JsonSerializerContext;
