using Moq;
using Moq.Protected;
using SendPulse.Client;
using SendPulse.Client.Common.Exceptions;
using SendPulse.Client.Templates;
using SendPulse.Client.Templates.Entities;
using System.Net;
using System.Text.Json;
using SendPulse.Client.Common.Entities;

namespace SendPulse.Tests.Unit.Services;

[TestClass]
public class TemplateServiceTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private HttpClient _httpClient = null!;
    private SendPulseClient _sendPulseClient = null!;
    private ITemplateService _templateService = null!;

    [TestInitialize]
    public void Setup()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _sendPulseClient = new SendPulseClient("test-id", "test-secret", "https://api.sendpulse.com", _httpClient);
        _templateService = _sendPulseClient.Templates;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _httpClient?.Dispose();
        _sendPulseClient?.Dispose();
    }

    [TestMethod]
    public async Task GetAsync_WithValidResponse_ShouldReturnTemplates()
    {
        // Arrange
        var templates = new List<Template>
        {
            new() { Id = "1", Name = "Template 1", Lang = "en" },
            new() { Id = "2", Name = "Template 2", Lang = "ru" }
        };

        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(templates));
        SetupAuthenticationResponse();

        // Act
        var result = await _templateService.GetAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Template 1", result[0].Name);
    }

    [TestMethod]
    public async Task GetAsync_WithOwnerFilter_ShouldIncludeOwnerInUrl()
    {
        // Arrange
        var templates = new List<Template>();
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(templates));
        SetupAuthenticationResponse();

        // Act
        await _templateService.GetAsync("me");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("owner=me")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [TestMethod]
    [ExpectedException(typeof(SendPulseException))]
    public async Task GetAsync_WithErrorResponse_ShouldThrowSendPulseException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "{\"error\":\"Bad request\"}");
        SetupAuthenticationResponse();

        // Act
        await _templateService.GetAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task CreateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act
        await _templateService.CreateAsync(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task CreateAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new CreateTemplateRequest
        {
            Name = "",
            Body = "<html>Test</html>",
            Language = Language.English
        };

        // Act
        await _templateService.CreateAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task CreateAsync_WithEmptyBody_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new CreateTemplateRequest
        {
            Name = "Test Template",
            Body = "",
            Language = Language.English
        };

        // Act
        await _templateService.CreateAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EditAsync_WithInvalidTemplateId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new UpdateTemplateRequest
        {
            Body = "<html>Test</html>",
            Language = Language.English
        };

        // Act
        await _templateService.EditAsync("", request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EditAsync_WithZeroRealId_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new UpdateTemplateRequest
        {
            Body = "<html>Test</html>",
            Language = Language.English
        };

        // Act
        await _templateService.EditAsync(0, request);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("template")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    private void SetupAuthenticationResponse()
    {
        var tokenResponse = new
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("oauth/access_token")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            });
    }
}

