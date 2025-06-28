using Microsoft.Extensions.Configuration;
using Moq;
using VideoManager.Application.Common;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Tests.Application
{
    public class TokenCommandTest
    {
        private const string ValidUser = "admin@fiap.com";
        private const string ValidPassword = "123456";
        private readonly string _validPasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword);

        private TokenCommand CreateCommand(Mock<ITokenService> tokenServiceMock = null, Mock<IConfiguration> configMock = null)
        {
            configMock ??= new Mock<IConfiguration>();
            configMock.Setup(c => c["Auth:User"]).Returns(ValidUser);
            configMock.Setup(c => c["Auth:Password"]).Returns(_validPasswordHash);

            tokenServiceMock ??= new Mock<ITokenService>();
            tokenServiceMock.Setup(s => s.GerarToken(It.IsAny<string>())).Returns("token123");

            return new TokenCommand(configMock.Object, tokenServiceMock.Object);
        }

        [Fact]
        public void Execute_ReturnsSuccess_WhenUserAndPasswordAreValid()
        {
            // Arrange
            var command = CreateCommand();
            var request = new LoginRequest { User = ValidUser, Password = ValidPassword };

            // Act
            var result = command.Execute(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("token123", result.Token);
            Assert.Null(result.MessageError);
        }

        [Fact]
        public void Execute_ReturnsFailure_WhenUserIsInvalid()
        {
            // Arrange
            var command = CreateCommand();
            var request = new LoginRequest { User = "wrong@fiap.com", Password = ValidPassword };

            // Act
            var result = command.Execute(request);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("Login ou senha inválidos", result.MessageError);
        }

        [Fact]
        public void Execute_ReturnsFailure_WhenPasswordIsInvalid()
        {
            // Arrange
            var command = CreateCommand();
            var request = new LoginRequest { User = ValidUser, Password = "wrongpassword" };

            // Act
            var result = command.Execute(request);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("Login ou senha inválidos", result.MessageError);
        }
    }
}