using System.Net.Http;
using SendPulse.Client.Authentication;
using SendPulse.Client.Templates;

namespace SendPulse.Client;

/// <summary>
/// Main entry point for SendPulse API client
/// </summary>
public class SendPulseClient : IDisposable
{
    private const string DefaultApiUrl = "https://api.sendpulse.com";
    private readonly bool _disposeHttpClient;
    private readonly Lazy<ITemplateService> _templateService;
    
    /// <summary>
    /// Initializes a new instance of the SendPulseClient
    /// </summary>
    /// <param name="clientId">SendPulse client ID (from API credentials)</param>
    /// <param name="clientSecret">SendPulse client secret (from API credentials)</param>
    /// <param name="baseUrl">Base URL for SendPulse API (default: https://api.sendpulse.com)</param>
    public SendPulseClient(string clientId, string clientSecret)
        : this(clientId, clientSecret, DefaultApiUrl, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the SendPulseClient
    /// </summary>
    /// <param name="clientId">SendPulse client ID (from API credentials)</param>
    /// <param name="clientSecret">SendPulse client secret (from API credentials)</param>
    /// <param name="baseUrl">Base URL for SendPulse API (default: https://api.sendpulse.com)</param>
    public SendPulseClient(string clientId, string clientSecret, string baseUrl = DefaultApiUrl)
        : this(clientId, clientSecret, baseUrl, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the SendPulseClient with custom HttpClient
    /// </summary>
    /// <param name="clientId">SendPulse client ID (from API credentials)</param>
    /// <param name="clientSecret">SendPulse client secret (from API credentials)</param>
    /// <param name="baseUrl">Base URL for SendPulse API (default: https://api.sendpulse.com)</param>
    /// <param name="httpClient">Custom HTTP client (optional)</param>
    public SendPulseClient(string clientId, string clientSecret, string baseUrl = DefaultApiUrl, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentNullException(nameof(clientId));
        
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentNullException(nameof(clientSecret));

        BaseUrl = !string.IsNullOrWhiteSpace(baseUrl) 
            ? baseUrl.TrimEnd('/') 
            : throw new ArgumentNullException(nameof(baseUrl));

        if (httpClient == null)
        {
            HttpClient = new HttpClient();
            _disposeHttpClient = true;
        }
        else
        {
            HttpClient = httpClient;
            _disposeHttpClient = false;
        }

        Authentication = new AuthenticationService(HttpClient, clientId, clientSecret, BaseUrl);
        _templateService = new Lazy<ITemplateService>(() => new TemplateService(this));
    }

    /// <summary>
    /// Gets the authentication service
    /// </summary>
    internal AuthenticationService Authentication { get; }

    /// <summary>
    /// Gets the base URL
    /// </summary>
    public string BaseUrl { get; }

    /// <summary>
    /// Gets the HTTP client used for requests
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the email template service
    /// </summary>
    public ITemplateService Templates => _templateService.Value;

    /// <summary>
    /// Executes an authenticated GET request
    /// </summary>
    protected internal async Task<HttpResponseMessage> GetAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, endpoint, cancellationToken);
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes an authenticated POST request
    /// </summary>
    protected internal async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, endpoint, cancellationToken);
        if (content != null)
        {
            request.Content = content;
        }
        
        // Debug: log the actual URL being called
        System.Diagnostics.Debug.WriteLine($"POST URL: {request.RequestUri}");
        System.Diagnostics.Debug.WriteLine($"Content-Type: {content?.Headers?.ContentType}");
        
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes an authenticated PUT request
    /// </summary>
    protected internal async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        var request = await CreateAuthenticatedRequestAsync(HttpMethod.Put, endpoint, cancellationToken);
        if (content != null)
        {
            request.Content = content;
        }
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes an authenticated DELETE request
    /// </summary>
    protected internal async Task<HttpResponseMessage> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, endpoint, cancellationToken);
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Creates an authenticated HTTP request
    /// </summary>
    private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(HttpMethod method, string endpoint, CancellationToken cancellationToken)
    {
        var token = await Authentication.GetAccessTokenAsync(cancellationToken);
        var url = endpoint.StartsWith("http") ? endpoint : $"{BaseUrl}/{endpoint.TrimStart('/')}";
        
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        return request;
    }

    /// <summary>
    /// Disposes the client and releases resources
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            HttpClient.Dispose();
        }
    }
}
