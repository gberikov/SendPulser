using System.Text.Json.Serialization;
using SendPulse.Client.Common.Entities;

namespace SendPulse.Client.Templates.Entities;

/// <summary>
/// Request model for updating an email template
/// </summary>
public record UpdateTemplateRequest
{
    [JsonIgnore]
    public Language Language { get; set; } = Language.English;

    [JsonPropertyName("lang")]
    public string LanguageString
    {
        get => Language.ToApiString();
        set => Language = LanguageExtensions.FromApiString(value);
    }

    [JsonIgnore]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string BodyBase64
    {
        get => string.IsNullOrEmpty(Body) ? string.Empty : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Body));
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    Body = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                }
                catch
                {
                    Body = value;
                }
            }
            else
            {
                Body = string.Empty;
            }
        }
    }
}
