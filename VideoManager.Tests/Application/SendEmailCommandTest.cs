using Moq;
using VideoManager.Application.Commands;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Tests.Application
{
    public class SendEmailCommandTest
    {
        [Fact]
        public async Task SendEmailAsync_CallsEmailServiceWithCorrectParameters()
        {
            // Arrange
            var emailServiceMock = new Mock<IEmailService>();
            var command = new SendEmailCommand(emailServiceMock.Object);

            string usuario = "user@email.com";
            string nomeArquivo = "video.mp4";
            VideoStatus status = VideoStatus.Erro;
            string assunto = "Erro no vídeo";
            string mensagem = "Falha ao processar o vídeo";

            // Act
            await command.SendEmailAsync(usuario, nomeArquivo, status, assunto, mensagem);

            // Assert
            emailServiceMock.Verify(s => s.EnviarEmailAsync(
                usuario,
                assunto,
                mensagem,
                It.Is<string>(html => html.Contains(nomeArquivo) && html.Contains(status.ToString()) && html.Contains(mensagem))
            ), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_Throws_WhenEmailServiceThrows()
        {
            // Arrange
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock
                .Setup(s => s.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("SMTP error"));

            var command = new SendEmailCommand(emailServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                command.SendEmailAsync("user@email.com", "video.mp4", VideoStatus.Erro, "Erro", "Falha")
            );
        }
    }
}