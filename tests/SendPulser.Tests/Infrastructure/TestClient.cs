namespace SendPulser.Tests.Infrastructure;

/// <summary>
/// Builds a client whose transport is a <see cref="FakeHttpMessageHandler"/> and whose requests are
/// already authenticated, so service tests are about endpoints and payloads only.
/// </summary>
internal static class TestClient
{
    public static readonly Uri BaseAddress = new("https://api.sendpulse.com/");

    public static (SendPulserClient Client, FakeHttpMessageHandler Handler) Create()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return (new SendPulserClient(httpClient), handler);
    }

    public static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
}

/// <summary>
/// A clock the tests move by hand.
/// </summary>
internal sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;

    public void Advance(TimeSpan by) => Now += by;
}
