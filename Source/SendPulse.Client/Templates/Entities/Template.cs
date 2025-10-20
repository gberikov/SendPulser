using System.Text.Json.Serialization;
using SendPulse.Client.Common.Converters;
using SendPulse.Client.Common.Entities;
using SendPulse.Client.Templates.Converters;

namespace SendPulse.Client.Templates.Entities;

/// <summary>
/// Represents an email template in SendPulse
/// </summary>
public class Template
{
    /// <summary>
    /// Template ID (GUID)
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Real numeric ID of the template
    /// </summary>
    [JsonPropertyName("real_id")]
    public int RealId { get; set; }

    /// <summary>
    /// Template language
    /// </summary>
    [JsonIgnore]
    public Language Language { get; set; } = Language.English;

    /// <summary>
    /// Template language as string (used for API serialization)
    /// </summary>
    [JsonPropertyName("lang")]
    public string Lang
    {
        get => Language.ToApiString();
        set => Language = LanguageExtensions.FromApiString(value);
    }

    /// <summary>
    /// Template name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template name slug (URL-friendly version)
    /// </summary>
    [JsonPropertyName("name_slug")]
    public string NameSlug { get; set; } = string.Empty;

    /// <summary>
    /// Date when template was created
    /// </summary>
    [JsonPropertyName("created")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? Created { get; set; }

    /// <summary>
    /// Full description of the template
    /// </summary>
    [JsonPropertyName("full_description")]
    public string? FullDescription { get; set; }

    /// <summary>
    /// Indicates if template is a structure
    /// </summary>
    [JsonPropertyName("is_structure")]
    public bool IsStructure { get; set; }

    /// <summary>
    /// Template category
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Category information
    /// </summary>
    [JsonPropertyName("category_info")]
    [JsonConverter(typeof(CategoryInfoConverter))]
    public CategoryInfo? CategoryInfo { get; set; }

    /// <summary>
    /// Template tags
    /// </summary>
    [JsonPropertyName("tags")]
    [JsonConverter(typeof(TagConverter))]
    public List<Tag> Tags { get; set; } = [];

    /// <summary>
    /// Template owner (e.g., "you", "sendpulse")
    /// </summary>
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// URL to template preview image
    /// </summary>
    [JsonPropertyName("preview")]
    public Uri? Preview { get; set; }
}