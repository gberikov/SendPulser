using System.Text.Json.Serialization;

namespace SendPulse.Client.Templates.Entities;

/// <summary>
/// Represents category information for an email template
/// </summary>
public class CategoryInfo
{
    /// <summary>
    /// Category ID
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Meta description for the category
    /// </summary>
    [JsonPropertyName("meta_description")]
    public string MetaDescription { get; set; } = string.Empty;

    /// <summary>
    /// Full description of the category
    /// </summary>
    [JsonPropertyName("full_description")]
    public string FullDescription { get; set; } = string.Empty;

    /// <summary>
    /// Category code
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Sort order
    /// </summary>
    [JsonPropertyName("sort")]
    public int Sort { get; set; }
}

