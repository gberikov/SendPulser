using SendPulse.Client.Common.Entities;

namespace SendPulse.Tests.Unit.Entities;

[TestClass]
public class TemplateLanguageExtensionsTests
{
    [TestMethod]
    public void ToApiString_ShouldReturnCorrectString_ForRussian()
    {
        // Arrange
        var language = Language.Russian;

        // Act
        var result = language.ToApiString();

        // Assert
        Assert.AreEqual("ru", result);
    }

    [TestMethod]
    public void ToApiString_ShouldReturnCorrectString_ForEnglish()
    {
        // Arrange
        var language = Language.English;

        // Act
        var result = language.ToApiString();

        // Assert
        Assert.AreEqual("en", result);
    }

    [TestMethod]
    public void ToApiString_ShouldReturnCorrectString_ForAllLanguages()
    {
        // Arrange & Act & Assert
        Assert.AreEqual("ru", Language.Russian.ToApiString());
        Assert.AreEqual("en", Language.English.ToApiString());
        Assert.AreEqual("ua", Language.Ukrainian.ToApiString());
        Assert.AreEqual("tr", Language.Turkish.ToApiString());
        Assert.AreEqual("es", Language.Spanish.ToApiString());
        Assert.AreEqual("pt", Language.Portuguese.ToApiString());
    }

    [TestMethod]
    public void FromApiString_ShouldReturnCorrectEnum_ForRussian()
    {
        // Arrange
        var apiString = "ru";

        // Act
        var result = LanguageExtensions.FromApiString(apiString);

        // Assert
        Assert.AreEqual(Language.Russian, result);
    }

    [TestMethod]
    public void FromApiString_ShouldReturnCorrectEnum_ForEnglish()
    {
        // Arrange
        var apiString = "en";

        // Act
        var result = LanguageExtensions.FromApiString(apiString);

        // Assert
        Assert.AreEqual(Language.English, result);
    }

    [TestMethod]
    public void FromApiString_ShouldReturnCorrectEnum_ForAllLanguages()
    {
        // Arrange & Act & Assert
        Assert.AreEqual(Language.Russian, LanguageExtensions.FromApiString("ru"));
        Assert.AreEqual(Language.English, LanguageExtensions.FromApiString("en"));
        Assert.AreEqual(Language.Ukrainian, LanguageExtensions.FromApiString("ua"));
        Assert.AreEqual(Language.Turkish, LanguageExtensions.FromApiString("tr"));
        Assert.AreEqual(Language.Spanish, LanguageExtensions.FromApiString("es"));
        Assert.AreEqual(Language.Portuguese, LanguageExtensions.FromApiString("pt"));
    }

    [TestMethod]
    public void FromApiString_ShouldReturnEnglish_ForUnknownLanguage()
    {
        // Arrange
        var apiString = "unknown";

        // Act
        var result = LanguageExtensions.FromApiString(apiString);

        // Assert
        Assert.AreEqual(Language.English, result);
    }

    [TestMethod]
    public void FromApiString_ShouldReturnEnglish_ForNull()
    {
        // Act
        var result = LanguageExtensions.FromApiString(null);

        // Assert
        Assert.AreEqual(Language.English, result);
    }

    [TestMethod]
    public void FromApiString_ShouldBeCaseInsensitive()
    {
        // Arrange & Act & Assert
        Assert.AreEqual(Language.Russian, LanguageExtensions.FromApiString("RU"));
        Assert.AreEqual(Language.Russian, LanguageExtensions.FromApiString("Ru"));
        Assert.AreEqual(Language.English, LanguageExtensions.FromApiString("EN"));
    }
}