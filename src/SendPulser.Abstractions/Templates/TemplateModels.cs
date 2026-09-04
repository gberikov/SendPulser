using System.Text.Json.Serialization;
using SendPulser.Json;

namespace SendPulser.Templates;

/// <summary>
/// An email template stored in SendPulse.
/// </summary>
public sealed class Template
{
    /// <summary>Template ID (a GUID-like string).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Numeric template ID. Either ID is accepted wherever a template is referenced.</summary>
    [JsonPropertyName("real_id")]
    public int RealId { get; set; }

    /// <summary>Template language: <c>en</c>, <c>ru</c>, <c>ua</c>, <c>tr</c>, <c>es</c> or <c>pt</c>.</summary>
    [JsonPropertyName("lang")]
    public string? Language { get; set; }

    /// <summary>Template name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL friendly template name.</summary>
    [JsonPropertyName("name_slug")]
    public string? NameSlug { get; set; }

    /// <summary>Creation date.</summary>
    [JsonPropertyName("created")]
    [JsonConverter(typeof(SendPulseDateTimeConverter))]
    public DateTime? CreatedAt { get; set; }

    /// <summary>Long description.</summary>
    [JsonPropertyName("full_description")]
    public string? FullDescription { get; set; }

    /// <summary>Whether the template is a structure.</summary>
    [JsonPropertyName("is_structure")]
    public bool IsStructure { get; set; }

    /// <summary>Category code.</summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Category details. SendPulse returns an empty array instead of an object when the template has no
    /// category, which is why this is read through <see cref="CategoryInfoConverter"/>.
    /// </summary>
    [JsonPropertyName("category_info")]
    [JsonConverter(typeof(CategoryInfoConverter))]
    public TemplateCategory? CategoryInfo { get; set; }

    /// <summary>Tags assigned to the template.</summary>
    [JsonPropertyName("tags")]
    [JsonConverter(typeof(TagCollectionConverter))]
    public IReadOnlyList<string> Tags { get; set; } = [];

    /// <summary>Owner of the template, typically <c>me</c> or <c>sendpulse</c>.</summary>
    [JsonPropertyName("owner")]
    public string? Owner { get; set; }

    /// <summary>Preview image URL.</summary>
    [JsonPropertyName("preview")]
    public Uri? Preview { get; set; }
}

/// <summary>
/// Category a template belongs to.
/// </summary>
public sealed class TemplateCategory
{
    /// <summary>Category ID.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Category name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Category code.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Meta description used by SendPulse.</summary>
    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; set; }

    /// <summary>Long description.</summary>
    [JsonPropertyName("full_description")]
    public string? FullDescription { get; set; }

    /// <summary>Sort order.</summary>
    [JsonPropertyName("sort")]
    public int Sort { get; set; }
}

/// <summary>
/// Payload for creating a template.
/// </summary>
public sealed class CreateTemplateRequest
{
    /// <summary>
    /// Template name. When omitted SendPulse names it <c>Template YYYY.mm.dd H:i:s</c>.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// HTML body. Pass plain markup: it is Base64 encoded on the wire, as SendPulse requires.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonConverter(typeof(Base64BodyConverter))]
    public string Body { get; set; } = string.Empty;

    /// <summary>Template language: <c>en</c>, <c>ru</c>, <c>ua</c>, <c>tr</c>, <c>es</c> or <c>pt</c>.</summary>
    [JsonPropertyName("lang")]
    public string? Language { get; set; }
}

/// <summary>
/// Payload for editing a template.
/// </summary>
public sealed class UpdateTemplateRequest
{
    /// <summary>
    /// HTML body. Pass plain markup: it is Base64 encoded on the wire, as SendPulse requires.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonConverter(typeof(Base64BodyConverter))]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Template language. SendPulse requires the same language the template was created with.
    /// </summary>
    [JsonPropertyName("lang")]
    public string? Language { get; set; }
}

/// <summary>
/// Result of creating a template.
/// </summary>
public sealed class CreateTemplateResult
{
    /// <summary>Whether SendPulse accepted the request.</summary>
    [JsonPropertyName("result")]
    public bool Result { get; set; }

    /// <summary>Numeric ID of the new template.</summary>
    [JsonPropertyName("real_id")]
    public int RealId { get; set; }
}
