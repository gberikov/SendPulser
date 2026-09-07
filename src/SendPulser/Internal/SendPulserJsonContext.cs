using System.Text.Json.Serialization;
using SendPulser.AddressBooks;
using SendPulser.Balance;
using SendPulser.Campaigns;
using SendPulser.EmailAddresses;
using SendPulser.Senders;
using SendPulser.Smtp;
using SendPulser.Tags;
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
[JsonSerializable(typeof(UpdateVariablesRequest))]
[JsonSerializable(typeof(SetPhoneRequest))]
[JsonSerializable(typeof(UnsubscribeListRequest))]
[JsonSerializable(typeof(ResubscribeRequest))]
[JsonSerializable(typeof(SendEmailEnvelope))]
[JsonSerializable(typeof(CreateWebhookRequest))]
[JsonSerializable(typeof(UpdateWebhookRequest))]
[JsonSerializable(typeof(SenderRequest))]
[JsonSerializable(typeof(ActivationCodeRequest))]
[JsonSerializable(typeof(BlacklistRequest))]
[JsonSerializable(typeof(TagRequest))]
[JsonSerializable(typeof(TagEmailRequest))]
[JsonSerializable(typeof(TagPhoneRequest))]
[JsonSerializable(typeof(TagListResponse))]
[JsonSerializable(typeof(AddressBook))]
[JsonSerializable(typeof(List<AddressBook>))]
[JsonSerializable(typeof(List<AddressBookVariable>))]
[JsonSerializable(typeof(List<Contact>))]
[JsonSerializable(typeof(ContactDetails))]
[JsonSerializable(typeof(List<AddressBookCampaign>))]
[JsonSerializable(typeof(CampaignCost))]
[JsonSerializable(typeof(List<EmailAddressMembership>))]
[JsonSerializable(typeof(Dictionary<string, IReadOnlyList<EmailAddressMembership>>))]
[JsonSerializable(typeof(List<EmailAddressListEntry>))]
[JsonSerializable(typeof(EmailAddressStatistics))]
[JsonSerializable(typeof(Dictionary<string, EmailAddressBatchStatistics>))]
[JsonSerializable(typeof(Template))]
[JsonSerializable(typeof(TemplateCategory))]
[JsonSerializable(typeof(List<Template>))]
[JsonSerializable(typeof(CreateTemplateRequest))]
[JsonSerializable(typeof(CreateTemplateResult))]
[JsonSerializable(typeof(UpdateTemplateRequest))]
[JsonSerializable(typeof(Campaign))]
[JsonSerializable(typeof(List<Campaign>))]
[JsonSerializable(typeof(CampaignInfo))]
[JsonSerializable(typeof(CampaignRecipient))]
[JsonSerializable(typeof(List<CampaignReferral>))]
[JsonSerializable(typeof(Dictionary<string, int>))]
[JsonSerializable(typeof(CreateCampaignRequest))]
[JsonSerializable(typeof(CampaignStatisticsSettings))]
[JsonSerializable(typeof(CreateCampaignResult))]
[JsonSerializable(typeof(UpdateCampaignRequest))]
[JsonSerializable(typeof(List<Sender>))]
[JsonSerializable(typeof(TagOperationResult))]
[JsonSerializable(typeof(AccountBalance))]
[JsonSerializable(typeof(BalanceDetails))]
[JsonSerializable(typeof(SendEmailResult))]
[JsonSerializable(typeof(SmtpEmail))]
[JsonSerializable(typeof(List<SmtpEmail>))]
[JsonSerializable(typeof(SmtpBouncePage))]
[JsonSerializable(typeof(List<UnsubscribedContact>))]
[JsonSerializable(typeof(List<SenderDomain>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Webhook))]
[JsonSerializable(typeof(List<Webhook>))]
internal sealed partial class SendPulserJsonContext : JsonSerializerContext;
