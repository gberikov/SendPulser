using SendPulse.Client.Common.Entities;
using SendPulse.Client.Templates.Entities;

namespace SendPulse.Tests.Unit.Entities;

[TestClass]
public class CreateTemplateRequestTests
{
    [TestMethod]
    public void BodyBase64_ShouldEncodeBodyToBase64()
    {
        // Arrange
        var request = new CreateTemplateRequest
        {
            Body = "<html><body>Test</body></html>"
        };

        // Act
        var base64 = request.BodyBase64;

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(base64));
        Assert.AreNotEqual(request.Body, base64);
    }

    [TestMethod]
    public void BodyBase64_ShouldDecodeBase64ToBody()
    {
        // Arrange
        var originalBody = "<html><body>Test</body></html>";
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(originalBody));
        
        var request = new CreateTemplateRequest
        {
            BodyBase64 = base64
        };

        // Act
        var decodedBody = request.Body;

        // Assert
        Assert.AreEqual(originalBody, decodedBody);
    }

    [TestMethod]
    public void BodyBase64_ShouldReturnEmptyString_WhenBodyIsEmpty()
    {
        // Arrange
        var request = new CreateTemplateRequest
        {
            Body = string.Empty
        };

        // Act
        var base64 = request.BodyBase64;

        // Assert
        Assert.AreEqual(string.Empty, base64);
    }

    [TestMethod]
    public void BodyBase64_ShouldHandleInvalidBase64Gracefully()
    {
        // Arrange
        var request = new CreateTemplateRequest();
        var invalidBase64 = "not-valid-base64!!!";

        // Act
        request.BodyBase64 = invalidBase64;

        // Assert
        Assert.AreEqual(invalidBase64, request.Body);
    }

    [TestMethod]
    public void LanguageString_ShouldConvertLanguageEnumToString()
    {
        // Arrange
        var request = new CreateTemplateRequest
        {
            Language = Language.Russian
        };

        // Act
        var langString = request.LanguageString;

        // Assert
        Assert.AreEqual("ru", langString);
    }

    [TestMethod]
    public void LanguageString_ShouldConvertStringToLanguageEnum()
    {
        // Arrange
        var request = new CreateTemplateRequest
        {
            LanguageString = "es"
        };

        // Act
        var language = request.Language;

        // Assert
        Assert.AreEqual(Language.Spanish, language);
    }

    [TestMethod]
    public void DefaultLanguage_ShouldBeEnglish()
    {
        // Arrange & Act
        var request = new CreateTemplateRequest();

        // Assert
        Assert.AreEqual(Language.English, request.Language);
    }

    [TestMethod]
    public void RoundTrip_BodyEncodeAndDecode_ShouldPreserveContent()
    {
        // Arrange
        var originalBody = "<html><head><title>Test</title></head><body><h1>Hello, мир!</h1></body></html>";
        var request = new CreateTemplateRequest
        {
            Body = originalBody
        };

        // Act
        var base64 = request.BodyBase64;
        var newRequest = new CreateTemplateRequest
        {
            BodyBase64 = base64
        };

        // Assert
        Assert.AreEqual(originalBody, newRequest.Body);
    }
}

