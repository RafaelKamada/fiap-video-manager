using Moq;
using VideoManager.Application.Commands;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Tests.Application;

public class UpdateStatusCommandTest
{
    [Fact]
    public async Task Execute_ReturnsSuccess_WhenVideoIsUpdated()
    {
        // Arrange
        var video = new VideoManager.Domain.Entities.Video
        {
            Id = 1,
            NomeArquivo = "video.mp4",
            CaminhoVideo = "video/path.mp4",
            Status = VideoStatus.Uploaded,
            Usuario = "user@email.com"
        };

        var repoMock = new Mock<IVideoRepository>();
        repoMock.Setup(r => r.Get(1)).ReturnsAsync(video);
        repoMock.Setup(r => r.Update(It.IsAny<VideoManager.Domain.Entities.Video>())).ReturnsAsync(video);

        var emailMock = new Mock<ISendEmailCommand>();
        var storageMock = new Mock<IStorageService>();

        var command = new UpdateStatusCommand(repoMock.Object, emailMock.Object, storageMock.Object);

        // Act
        var result = await command.Execute(1, "zip/path.zip", VideoStatus.Finalizado);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(video.Id, result.VideoId);
        Assert.Equal(video.NomeArquivo, result.NomeArquivo);
        Assert.Equal(VideoStatus.Finalizado, result.Status);
    }

    [Fact]
    public async Task Execute_ReturnsFailure_WhenVideoNotFound()
    {
        var repoMock = new Mock<IVideoRepository>();
        repoMock.Setup(r => r.Get(1)).ReturnsAsync((VideoManager.Domain.Entities.Video)null);

        var emailMock = new Mock<ISendEmailCommand>();
        var storageMock = new Mock<IStorageService>();

        var command = new UpdateStatusCommand(repoMock.Object, emailMock.Object, storageMock.Object);

        var result = await command.Execute(1, "zip/path.zip", VideoStatus.Finalizado);

        Assert.False(result.Success);
        Assert.Equal("Vídeo não encontrado ou sem conteúdo.", result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_ReturnsFailure_WhenCaminhoVideoIsNull()
    {
        var video = new VideoManager.Domain.Entities.Video
        {
            Id = 1,
            NomeArquivo = "video.mp4",
            CaminhoVideo = null,
            Status = VideoStatus.Uploaded,
            Usuario = "user@email.com"
        };

        var repoMock = new Mock<IVideoRepository>();
        repoMock.Setup(r => r.Get(1)).ReturnsAsync(video);

        var emailMock = new Mock<ISendEmailCommand>();
        var storageMock = new Mock<IStorageService>();

        var command = new UpdateStatusCommand(repoMock.Object, emailMock.Object, storageMock.Object);

        var result = await command.Execute(1, "zip/path.zip", VideoStatus.Finalizado);

        Assert.False(result.Success);
        Assert.Equal("Vídeo não encontrado ou sem conteúdo.", result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_SendsEmailAndReturnsFailure_OnException()
    {
        var video = new VideoManager.Domain.Entities.Video
        {
            Id = 1,
            NomeArquivo = "video.mp4",
            CaminhoVideo = "video/path.mp4",
            Status = VideoStatus.Uploaded,
            Usuario = "user@email.com"
        };

        var repoMock = new Mock<IVideoRepository>();
        repoMock.Setup(r => r.Get(1)).ReturnsAsync(video);
        repoMock.Setup(r => r.Update(It.IsAny<VideoManager.Domain.Entities.Video>())).ThrowsAsync(new Exception("DB error"));

        var emailMock = new Mock<ISendEmailCommand>();
        emailMock.Setup(e => e.SendEmailAsync(
            video.Usuario, video.Id.ToString(), VideoStatus.Erro, "Erro ao atualizar vídeo", "DB error"
        )).Returns(Task.CompletedTask).Verifiable();

        var storageMock = new Mock<IStorageService>();

        var command = new UpdateStatusCommand(repoMock.Object, emailMock.Object, storageMock.Object);

        var result = await command.Execute(1, "zip/path.zip", VideoStatus.Finalizado);

        Assert.False(result.Success);
        Assert.Equal("DB error", result.ErrorMessage);
        emailMock.Verify();
    }
}