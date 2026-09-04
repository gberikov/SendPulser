using SendPulser.AddressBooks;
using SendPulser.Campaigns;
using SendPulser.Smtp;
using SendPulser.Templates;
using SendPulser.Webhooks;

namespace SendPulser;

/// <summary>
/// Entry point to the SendPulse email API.
/// </summary>
public interface ISendPulserClient
{
    /// <summary>Mailing lists and the contacts in them.</summary>
    IAddressBookService AddressBooks { get; }

    /// <summary>Email templates.</summary>
    ITemplateService Templates { get; }

    /// <summary>Bulk email campaigns.</summary>
    ICampaignService Campaigns { get; }

    /// <summary>Transactional email through the SMTP service.</summary>
    ISmtpService Smtp { get; }

    /// <summary>Webhook registrations of the account.</summary>
    IWebhookService Webhooks { get; }
}
