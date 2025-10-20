using SendPulse.Client.Authentication.Entities;
using SendPulse.Client.Common.Entities;

namespace SendPulse.Tests.Unit.Authentication;

[TestClass]
public class AuthenticationTokenTests
{
    [TestMethod]
    public void IsExpired_WithFreshToken_ShouldReturnFalse()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            AccessToken = "test-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var isExpired = token.IsExpired;

        // Assert
        Assert.IsFalse(isExpired);
    }

    [TestMethod]
    public void IsExpired_WithExpiredToken_ShouldReturnTrue()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            AccessToken = "test-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            CreatedAt = DateTime.UtcNow.AddHours(-2) // Created 2 hours ago
        };

        // Act
        var isExpired = token.IsExpired;

        // Assert
        Assert.IsTrue(isExpired);
    }

    [TestMethod]
    public void IsExpired_WithTokenExpiringSoon_ShouldReturnTrue()
    {
        // Arrange
        var token = new AuthenticationToken
        {
            AccessToken = "test-token",
            TokenType = "Bearer",
            ExpiresIn = 120, // 2 minutes
            CreatedAt = DateTime.UtcNow.AddSeconds(-90) // Created 90 seconds ago
        };

        // Act
        var isExpired = token.IsExpired;

        // Assert
        // Should be considered expired due to 60-second buffer
        Assert.IsTrue(isExpired);
    }

    [TestMethod]
    public void CreatedAt_DefaultValue_ShouldBeUtcNow()
    {
        // Arrange & Act
        var token = new AuthenticationToken
        {
            AccessToken = "test-token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Assert
        Assert.IsTrue((DateTime.UtcNow - token.CreatedAt).TotalSeconds < 1);
    }
}

