using SendPulser.AddressBooks;
using SendPulser.Balance;
using SendPulser.Blacklist;
using SendPulser.Campaigns;
using SendPulser.EmailAddresses;
using SendPulser.Senders;
using SendPulser.Smtp;
using SendPulser.Tags;
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

    /// <summary>Email addresses across every mailing list.</summary>
    IEmailAddressService EmailAddresses { get; }

    /// <summary>Email templates.</summary>
    ITemplateService Templates { get; }

    /// <summary>Bulk email campaigns.</summary>
    ICampaignService Campaigns { get; }

    /// <summary>Sender addresses.</summary>
    ISenderService Senders { get; }

    /// <summary>The account blacklist.</summary>
    IBlacklistService Blacklist { get; }

    /// <summary>Tags and their assignment to contacts.</summary>
    ITagService Tags { get; }

    /// <summary>Balance and plans of the account.</summary>
    IBalanceService Balance { get; }

    /// <summary>Transactional email through the SMTP service.</summary>
    ISmtpService Smtp { get; }

    /// <summary>Webhook registrations of the account.</summary>
    IWebhookService Webhooks { get; }
}
