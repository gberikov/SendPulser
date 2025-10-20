using SendPulse.Client.Templates.Entities;

namespace SendPulse.Client.Common.Entities;

/// <summary>
/// Extension methods for TemplateLanguage enum
/// </summary>
public static class LanguageExtensions
{
    /// <summary>
    /// Converts TemplateLanguage enum to string value for API
    /// </summary>
    public static string ToApiString(this Language language)
    {
        return language switch
        {
            Language.Russian => "ru",
            Language.English => "en",
            Language.Ukrainian => "ua",
            Language.Turkish => "tr",
            Language.Spanish => "es",
            Language.Portuguese => "pt",
            _ => "en"
        };
    }

    /// <summary>
    /// Converts string value from API to TemplateLanguage enum
    /// </summary>
    public static Language FromApiString(string? language)
    {
        return language?.ToLowerInvariant() switch
        {
            "ru" => Language.Russian,
            "en" => Language.English,
            "ua" => Language.Ukrainian,
            "tr" => Language.Turkish,
            "es" => Language.Spanish,
            "pt" => Language.Portuguese,
            _ => Language.English
        };
    }
}


