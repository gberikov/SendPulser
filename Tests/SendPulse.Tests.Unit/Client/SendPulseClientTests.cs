using SendPulse.Client;

namespace SendPulse.Tests.Unit.Client;

[TestClass]
public class SendPulseClientTests
{
    [TestMethod]
    public void Constructor_WithValidCredentials_ShouldCreateClient()
    {
        // Arrange
        var clientId = "test-client-id";
        var clientSecret = "test-client-secret";

        // Act
        using var client = new SendPulseClient(clientId, clientSecret);

        // Assert
        Assert.IsNotNull(client);
        Assert.IsNotNull(client.Templates);
        Assert.AreEqual("https://api.sendpulse.com", client.BaseUrl);
    }

    [TestMethod]
    public void Constructor_WithCustomBaseUrl_ShouldUseProvidedUrl()
    {
        // Arrange
        var clientId = "test-client-id";
        var clientSecret = "test-client-secret";
        var customUrl = "https://custom.api.com";

        // Act
        using var client = new SendPulseClient(clientId, clientSecret, customUrl);

        // Assert
        Assert.AreEqual(customUrl, client.BaseUrl);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullClientId_ShouldThrowArgumentNullException()
    {
        // Act
        using var client = new SendPulseClient(null!, "secret");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithEmptyClientId_ShouldThrowArgumentNullException()
    {
        // Act
        using var client = new SendPulseClient("", "secret");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullClientSecret_ShouldThrowArgumentNullException()
    {
        // Act
        using var client = new SendPulseClient("client-id", null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithEmptyClientSecret_ShouldThrowArgumentNullException()
    {
        // Act
        using var client = new SendPulseClient("client-id", "");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullBaseUrl_ShouldThrowArgumentNullException()
    {
        // Act
        using var client = new SendPulseClient("client-id", "secret", null!);
    }

    [TestMethod]
    public void BaseUrl_ShouldRemoveTrailingSlash()
    {
        // Arrange
        var clientId = "test-client-id";
        var clientSecret = "test-client-secret";
        var urlWithSlash = "https://api.sendpulse.com/";

        // Act
        using var client = new SendPulseClient(clientId, clientSecret, urlWithSlash);

        // Assert
        Assert.AreEqual("https://api.sendpulse.com", client.BaseUrl);
    }

    [TestMethod]
    public void Templates_ShouldReturnSameInstance()
    {
        // Arrange
        using var client = new SendPulseClient("client-id", "secret");

        // Act
        var templates1 = client.Templates;
        var templates2 = client.Templates;

        // Assert
        Assert.AreSame(templates1, templates2);
    }

    [TestMethod]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var client = new SendPulseClient("client-id", "secret");

        // Act & Assert
        client.Dispose();
        // Should not throw
    }

    [TestMethod]
    public void Constructor_WithCustomHttpClient_ShouldUseProvidedClient()
    {
        // Arrange
        var clientId = "test-client-id";
        var clientSecret = "test-client-secret";
        var httpClient = new HttpClient();

        // Act
        using var client = new SendPulseClient(clientId, clientSecret, "https://api.sendpulse.com", httpClient);

        // Assert
        Assert.IsNotNull(client);
        Assert.IsNotNull(client.Templates);
    }
}

