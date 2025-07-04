using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VideoManager.Infrastructure.Services;

namespace VideoManager.Tests.Infrastructure.Service;

public class TokenServiceTest
{
    private static IConfiguration GetMockConfig(
        string key = "supersecretkey1234567890",
        string expiry = "60",
        string issuer = "TestIssuer",
        string audience = "TestAudience")
    {
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c["JwtSettings:Key"]).Returns(key);
        mock.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns(expiry);
        mock.Setup(c => c["JwtSettings:Issuer"]).Returns(issuer);
        mock.Setup(c => c["JwtSettings:Audience"]).Returns(audience);
        return mock.Object;
    }

    [Fact]
    public void GerarToken_Throws_When_Config_Missing()
    {
        // Arrange
        var mock = new Mock<IConfiguration>();
        var service = new TokenService(mock.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.GerarToken("user@email.com"));
    }

    [Fact]
    public void GerarToken_Throws_When_Expiry_Invalid()
    {
        // Arrange
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c["JwtSettings:Key"]).Returns("key");
        mock.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns("notanumber");
        mock.Setup(c => c["JwtSettings:Issuer"]).Returns("issuer");
        mock.Setup(c => c["JwtSettings:Audience"]).Returns("audience");
        var service = new TokenService(mock.Object);

        // Act & Assert
        Assert.Throws<FormatException>(() => service.GerarToken("user@email.com"));
    }
}