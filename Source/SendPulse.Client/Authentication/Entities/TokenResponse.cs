using System.Text.Json.Serialization;

namespace SendPulse.Client.Authentication.Entities;

internal class TokenResponse
{
    /// <summary>
    /// Access token for API requests
    /// </summary>
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (usually "Bearer")
    /// </summary>
    [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}