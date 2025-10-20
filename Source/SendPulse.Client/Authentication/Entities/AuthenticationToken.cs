namespace SendPulse.Client.Authentication.Entities;

/// <summary>
/// Represents an OAuth authentication token from SendPulse API
/// </summary>
public record AuthenticationToken
{
    /// <summary>
    /// Access token for API requests
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (usually "Bearer")
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Timestamp when the token was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if the token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= CreatedAt.AddSeconds(ExpiresIn - 60); // 60 seconds buffer
}