using System.Text.Json.Serialization;
using SendPulser.AddressBooks;
using SendPulser.Smtp;
using SendPulser.Tags;

namespace SendPulser.Internal;

internal sealed class TokenRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "client_credentials";

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;
}

internal sealed class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

/// <summary>
/// The union of the error shapes SendPulse returns: the <c>is_error</c> envelope of the email API and
/// the OAuth style <c>error</c> pair of the token endpoint.
/// </summary>
internal sealed class ApiError
{
    [JsonPropertyName("is_error")]
    public bool IsError { get; set; }

    [JsonPropertyName("http_code")]
    public int? HttpCode { get; set; }

    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}

internal sealed class ResultResponse
{
    [JsonPropertyName("result")]
    public bool Result { get; set; }
}

internal sealed class IdResponse
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public int Id { get; set; }
}

internal sealed class TotalResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

internal sealed class CreateAddressBookRequest
{
    [JsonPropertyName("bookName")]
    public string BookName { get; set; } = string.Empty;
}

internal sealed class RenameAddressBookRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

internal sealed class AddContactsRequest
{
    [JsonPropertyName("emails")]
    public IReadOnlyList<NewContact> Emails { get; set; } = [];

    [JsonPropertyName("tags")]
    public IReadOnlyList<int>? Tags { get; set; }

    [JsonPropertyName("confirmation")]
    public string? Confirmation { get; set; }

    [JsonPropertyName("sender_email")]
    public string? SenderEmail { get; set; }

    [JsonPropertyName("message_lang")]
    public string? MessageLanguage { get; set; }

    [JsonPropertyName("template_id")]
    public string? TemplateId { get; set; }
}

internal sealed class EmailListRequest
{
    [JsonPropertyName("emails")]
    public IReadOnlyList<string> Emails { get; set; } = [];
}

internal sealed class UpdateVariablesRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("variables")]
    public IReadOnlyList<VariableUpdate> Variables { get; set; } = [];
}

internal sealed class SetPhoneRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;
}

internal sealed class UnsubscribeListRequest
{
    [JsonPropertyName("emails")]
    public IReadOnlyList<UnsubscribeRequest> Emails { get; set; } = [];
}

internal sealed class ResubscribeRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("sender")]
    public string Sender { get; set; } = string.Empty;

    [JsonPropertyName("lang")]
    public string? Language { get; set; }
}

internal sealed class SendEmailEnvelope
{
    [JsonPropertyName("email")]
    public SendEmailRequest Email { get; set; } = new();
}

internal sealed class CreateWebhookRequest
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("actions")]
    public IReadOnlyList<string> Actions { get; set; } = [];
}

internal sealed class UpdateWebhookRequest
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

internal sealed class SenderRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal sealed class ActivationCodeRequest
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// The blacklist endpoints take the addresses as one comma separated string, Base64 encoded.
/// </summary>
internal sealed class BlacklistRequest
{
    [JsonPropertyName("emails")]
    public string Emails { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

internal sealed class TagRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

internal sealed class TagEmailRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public IReadOnlyList<int> Tags { get; set; } = [];
}

internal sealed class TagPhoneRequest
{
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public IReadOnlyList<int> Tags { get; set; } = [];
}

internal sealed class TagListResponse
{
    [JsonPropertyName("tags")]
    public List<Tag> Tags { get; set; } = [];
}
