using SendPulse.Client.Common.Exceptions;

namespace SendPulse.Tests.Unit.Exceptions;

[TestClass]
public class SendPulseExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new SendPulseException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new SendPulseException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreEqual(innerException, exception.InnerException);
    }

    [TestMethod]
    public void Constructor_Default_ShouldCreateException()
    {
        // Act
        var exception = new SendPulseException();

        // Assert
        Assert.IsNotNull(exception);
    }
}

