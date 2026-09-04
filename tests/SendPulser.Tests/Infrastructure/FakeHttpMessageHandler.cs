using System.Net;
using System.Text;

namespace SendPulser.Tests.Infrastructure;

/// <summary>
/// Records the requests the client makes and replays canned responses, so the HTTP layer is exercised
/// without a network.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<RecordedRequest> Requests { get; } = [];

    public RecordedRequest LastRequest => Requests[^1];

    public FakeHttpMessageHandler RespondWith(
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string contentType = "application/json")
    {
        _responses.Enqueue(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, contentType),
        });

        return this;
    }

    public FakeHttpMessageHandler Respond(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responses.Enqueue(responder);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Add(new RecordedRequest(
            request.Method,
            request.RequestUri,
            body,
            request.Headers.Authorization?.Parameter));

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException($"No response was queued for {request.Method} {request.RequestUri}.");
        }

        return _responses.Dequeue()(request);
    }
}

internal sealed record RecordedRequest(HttpMethod Method, Uri? Uri, string? Body, string? BearerToken)
{
    public string Path => Uri?.AbsolutePath ?? string.Empty;

    public string Query => Uri?.Query ?? string.Empty;
}
