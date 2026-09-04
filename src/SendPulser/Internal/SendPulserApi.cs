using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SendPulser.Internal;

/// <summary>
/// The single place where requests are issued and SendPulse failures are turned into exceptions.
/// </summary>
internal sealed class SendPulserApi(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public Task<T> GetAsync<T>(string path, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Get, path, content: null, typeInfo, cancellationToken);

    public async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        HttpContent? content,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        var body = await SendRawAsync(method, path, content, cancellationToken).ConfigureAwait(false);
        var value = Deserialize(body, typeInfo);
        return value ?? throw new SendPulserApiException(
            $"SendPulse returned an empty body for {method} {path}.",
            HttpStatusCode.OK,
            errorCode: null,
            body);
    }

    /// <summary>
    /// Issues a request whose only meaningful answer is <c>{"result": true}</c>.
    /// </summary>
    public async Task SendAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var body = await SendRawAsync(method, path, content, cancellationToken).ConfigureAwait(false);
        var result = Deserialize(body, SendPulserJsonContext.Default.ResultResponse);
        if (result is { Result: false })
        {
            throw new SendPulserApiException(
                $"SendPulse rejected {method} {path}.",
                HttpStatusCode.OK,
                errorCode: null,
                body);
        }
    }

    public async Task<string> SendRawAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, new Uri(path, UriKind.Relative));
        request.Content = content;

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw BuildException(response.StatusCode, body, method, path);
        }

        // SendPulse reports part of its failures with HTTP 200 and an is_error envelope. The cheap string
        // probe keeps the happy path from parsing every payload twice.
        if (body.Contains("\"is_error\"", StringComparison.Ordinal))
        {
            var error = TryParseError(body);
            if (error is { IsError: true })
            {
                throw BuildException(response.StatusCode, body, method, path);
            }
        }

        return body;
    }

    public static JsonContent Json<TBody>(TBody body, JsonTypeInfo<TBody> typeInfo) =>
        JsonContent.Create(body, typeInfo);

    public static string BuildQuery(params (string Name, string? Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var parts = parameters
            .Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            .Select(parameter => $"{parameter.Name}={Uri.EscapeDataString(parameter.Value!)}")
            .ToArray();

        return parts.Length == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    public static string? Number(int? value) =>
        value?.ToString(CultureInfo.InvariantCulture);

    public static string? Date(DateOnly? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static T? Deserialize<T>(string body, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            return JsonSerializer.Deserialize(body, typeInfo);
        }
        catch (JsonException exception)
        {
            throw new SendPulserApiException(
                "SendPulse returned a body that does not match the expected shape.",
                exception);
        }
    }

    private static ApiError? TryParseError(string body)
    {
        try
        {
            return JsonSerializer.Deserialize(body, SendPulserJsonContext.Default.ApiError);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static SendPulserApiException BuildException(
        HttpStatusCode statusCode,
        string body,
        HttpMethod method,
        string path)
    {
        var error = TryParseError(body);
        var description = error?.Message
            ?? error?.ErrorDescription
            ?? error?.Error
            ?? (body.Length == 0 ? "no response body" : Truncate(body));

        var message = $"SendPulse returned {(int)statusCode} for {method} {path}: {description}";

        if (error?.ErrorCode == RateLimit.PerSecondErrorCode)
        {
            message += " (the hard limit of 10 requests per second was exceeded)";
        }

        return new SendPulserApiException(message, statusCode, error?.ErrorCode, body);
    }

    private static string Truncate(string body) =>
        body.Length <= 512 ? body : string.Concat(body.AsSpan(0, 512), "...");
}
