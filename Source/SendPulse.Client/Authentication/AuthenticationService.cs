using System.Text;
using System.Text.Json;
using SendPulse.Client.Authentication.Entities;
using SendPulse.Client.Common.Exceptions;

namespace SendPulse.Client.Authentication;

/// <summary>
/// Service responsible for OAuth authentication with SendPulse API
/// </summary>
public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _baseUrl;
    private AuthenticationToken? _currentToken;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the AuthenticationService
    /// </summary>
    /// <param name="httpClient">HTTP client for making requests</param>
    /// <param name="clientId">SendPulse client ID</param>
    /// <param name="clientSecret">SendPulse client secret</param>
    /// <param name="baseUrl">Base URL for SendPulse API</param>
    public AuthenticationService(HttpClient httpClient, string clientId, string clientSecret, string baseUrl)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _clientId = !string.IsNullOrWhiteSpace(clientId) 
            ? clientId 
            : throw new ArgumentNullException(nameof(clientId));
        _clientSecret = !string.IsNullOrWhiteSpace(clientSecret) 
            ? clientSecret 
            : throw new ArgumentNullException(nameof(clientSecret));
        _baseUrl = !string.IsNullOrWhiteSpace(baseUrl) 
            ? baseUrl.TrimEnd('/') 
            : throw new ArgumentNullException(nameof(baseUrl));
    }

    /// <summary>
    /// Gets a valid access token, refreshing if necessary
    /// </summary>
    /// <returns>Valid access token</returns>
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Check if we have a valid token without locking
        if (_currentToken is { IsExpired: false })
        {
            return _currentToken.AccessToken;
        }

        // Lock to prevent multiple simultaneous token requests
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_currentToken is { IsExpired: false })
            {
                return _currentToken.AccessToken;
            }

            // Request new token
            _currentToken = await RequestTokenAsync(cancellationToken);
            return _currentToken.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <summary>
    /// Requests a new OAuth token from SendPulse API
    /// </summary>
    private async Task<AuthenticationToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var requestData = new
        {
            grant_type = "client_credentials",
            client_id = _clientId,
            client_secret = _clientSecret
        };

        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"{_baseUrl}/oauth/access_token",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new SendPulseException(
                $"Failed to authenticate with SendPulse API. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new SendPulseException("Invalid token response from SendPulse API");
        }

        return new AuthenticationToken
        {
            AccessToken = tokenResponse.AccessToken,
            TokenType = tokenResponse.TokenType ?? "Bearer",
            ExpiresIn = tokenResponse.ExpiresIn,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Invalidates the current token, forcing a refresh on next request
    /// </summary>
    public void InvalidateToken()
    {
        _currentToken = null;
    }
}

