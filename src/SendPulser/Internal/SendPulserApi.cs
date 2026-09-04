using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SendPulser.Internal;

/// <summary>
/// The single place where requests are issued and SendPulse failures are turned into exceptions.
/// Bodies are handled as UTF-8 bytes end to end; text is only produced for error reporting.
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
            Text(body));
    }

    /// <summary>
    /// Issues a request whose only meaningful answer is a success flag: <c>{"result": true}</c>, the
    /// <c>{"data": {"result": true}}</c> envelope or the <c>{"success": true}</c> envelope of the v2 endpoints.
    /// </summary>
    public async Task SendAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var body = await SendRawAsync(method, path, content, cancellationToken).ConfigureAwait(false);
        EnsureAccepted(body, method, path);
    }

    /// <summary>
    /// Issues a request and returns the payload found under the <c>data</c> envelopes of the v2 endpoints,
    /// after checking their <c>success</c> and <c>result</c> flags.
    /// </summary>
    public async Task<T> SendEnvelopedAsync<T>(
        HttpMethod method,
        string path,
        HttpContent? content,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        var body = await SendRawAsync(method, path, content, cancellationToken).ConfigureAwait(false);
        using var document = ParseDocument(body);
        var payload = Unwrap(document.RootElement, body, method, path);

        try
        {
            return payload.Deserialize(typeInfo) ?? throw new SendPulserApiException(
                $"SendPulse returned an empty payload for {method} {path}.",
                HttpStatusCode.OK,
                errorCode: null,
                Text(body));
        }
        catch (JsonException exception)
        {
            throw ShapeMismatch(exception);
        }
    }

    public async Task<byte[]> SendRawAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, new Uri(path, UriKind.Relative));
        request.Content = content;

        HttpResponseMessage response;
        byte[] body;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw new SendPulserTransportException(
                $"The request {method} {path} to SendPulse failed before a response arrived: {exception.Message}",
                exception);
        }

        using (response)
        {
            try
            {
                body = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                throw new SendPulserTransportException(
                    $"The response to {method} {path} from SendPulse could not be read: {exception.Message}",
                    exception);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw BuildException(response.StatusCode, body, method, path);
            }
        }

        // SendPulse reports part of its failures with HTTP 200 and an is_error envelope. The cheap byte
        // probe keeps the happy path from parsing every payload twice.
        if (body.AsSpan().IndexOf("\"is_error\""u8) >= 0 && TryParseError(body) is { IsError: true })
        {
            throw BuildException(HttpStatusCode.OK, body, method, path);
        }

        return body;
    }

    public static JsonContent Json<TBody>(TBody body, JsonTypeInfo<TBody> typeInfo) =>
        JsonContent.Create(body, typeInfo);

    public static string BuildQuery(params (string Name, string? Value)[] parameters) =>
        BuildQuery((IEnumerable<(string Name, string? Value)>)parameters);

    public static string BuildQuery(IEnumerable<(string Name, string? Value)> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var builder = new StringBuilder();
        foreach (var (name, value) in parameters)
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            builder.Append(builder.Length == 0 ? '?' : '&')
                .Append(Uri.EscapeDataString(name))
                .Append('=')
                .Append(Uri.EscapeDataString(value));
        }

        return builder.ToString();
    }

    public static string Number(int? value) =>
        value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    public static string Number(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    public static string? Flag(bool? value) =>
        value == true ? "1" : null;

    public static string? Date(DateOnly? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string Segment(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return Uri.EscapeDataString(value);
    }

    public static string Text(byte[] body) => Encoding.UTF8.GetString(body);

    /// <summary>
    /// Follows the <c>data</c> envelopes of the v2 endpoints down to the payload, failing on an explicit
    /// <c>success: false</c> or <c>result: false</c> along the way.
    /// </summary>
    public static JsonElement Unwrap(JsonElement root, byte[] body, HttpMethod method, string path)
    {
        var element = root;
        while (element.ValueKind is JsonValueKind.Object)
        {
            if (IsExplicitlyFalse(element, "success") || IsExplicitlyFalse(element, "result"))
            {
                throw Rejected(element, body, method, path);
            }

            if (!element.TryGetProperty("data", out var inner))
            {
                break;
            }

            element = inner;
        }

        return element;
    }

    public static JsonDocument ParseDocument(byte[] body)
    {
        try
        {
            return JsonDocument.Parse(body);
        }
        catch (JsonException exception)
        {
            throw ShapeMismatch(exception);
        }
    }

    private static void EnsureAccepted(byte[] body, HttpMethod method, string path)
    {
        using var document = ParseDocument(body);
        var element = document.RootElement;
        var accepted = false;

        while (element.ValueKind is JsonValueKind.Object)
        {
            if (IsExplicitlyFalse(element, "success") || IsExplicitlyFalse(element, "result"))
            {
                throw Rejected(element, body, method, path);
            }

            accepted |= IsExplicitlyTrue(element, "success") || IsExplicitlyTrue(element, "result");

            if (!element.TryGetProperty("data", out var inner))
            {
                break;
            }

            element = inner;
        }

        // The v2 delete and update endpoints answer {"success": true, "data": [true]}.
        if (!accepted && element.ValueKind is JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind is JsonValueKind.False)
                {
                    throw Rejected(document.RootElement, body, method, path);
                }

                accepted |= item.ValueKind is JsonValueKind.True;
            }
        }

        if (!accepted)
        {
            throw new SendPulserApiException(
                $"SendPulse answered {method} {path} without a result flag; the body was: {Truncate(Text(body))}",
                HttpStatusCode.OK,
                errorCode: null,
                Text(body));
        }
    }

    private static bool IsExplicitlyFalse(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.False;

    private static bool IsExplicitlyTrue(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True;

    private static SendPulserApiException Rejected(JsonElement element, byte[] body, HttpMethod method, string path)
    {
        string? reason = null;
        if (element.TryGetProperty("error", out var error) && error.ValueKind is JsonValueKind.String)
        {
            reason = error.GetString();
        }
        else if (element.TryGetProperty("message", out var message) && message.ValueKind is JsonValueKind.String)
        {
            reason = message.GetString();
        }
        else if (element.TryGetProperty("description", out var description) && description.ValueKind is JsonValueKind.String)
        {
            reason = description.GetString();
        }

        return new SendPulserApiException(
            reason is null
                ? $"SendPulse rejected {method} {path}."
                : $"SendPulse rejected {method} {path}: {reason}",
            HttpStatusCode.OK,
            errorCode: null,
            Text(body));
    }

    private static T? Deserialize<T>(byte[] body, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            return JsonSerializer.Deserialize(body, typeInfo);
        }
        catch (JsonException exception)
        {
            throw ShapeMismatch(exception);
        }
    }

    private static SendPulserApiException ShapeMismatch(JsonException exception) =>
        new("SendPulse returned a body that does not match the expected shape.", exception);

    private static ApiError? TryParseError(byte[] body)
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
        byte[] body,
        HttpMethod method,
        string path)
    {
        var error = TryParseError(body);
        var text = Text(body);
        var description = error?.Message
            ?? error?.ErrorDescription
            ?? error?.Error
            ?? (text.Length == 0 ? "no response body" : Truncate(text));

        var message = $"SendPulse returned {(int)statusCode} for {method} {path}: {description}";

        if (error?.ErrorCode == RateLimit.PerSecondErrorCode)
        {
            message += " (the hard limit of 10 requests per second was exceeded)";
        }

        return new SendPulserApiException(message, statusCode, error?.ErrorCode, text);
    }

    private static string Truncate(string body) =>
        body.Length <= 512 ? body : string.Concat(body.AsSpan(0, 512), "...");
}
