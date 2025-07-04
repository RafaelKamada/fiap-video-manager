using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VideoManager.Application.Commands;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Tests.Application;

public class AddVideoCommandTest
{
    private static IFormFile CreateFakeFormFile(string fileName = "video.mp4", int length = 100)
    {
        var ms = new MemoryStream(new byte[length]);
        return new FormFile(ms, 0, length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "video/mp4"
        };
    }

    private static Video CreateFakeVideo(string usuario = "user@email.com", string nomeArquivo = "video.mp4")
    {
        return new Video
        {
            Id = 1,
            NomeArquivo = nomeArquivo,
            Usuario = usuario,
            Status = VideoStatus.Uploaded,
            CaminhoVideo = "s3://bucket/video.mp4"
        };
    }

    [Fact]
    public async Task Execute_ReturnsFailure_WhenFileIsNull()
    {
        var repoMock = new Mock<IVideoRepository>();
        var fileStorageMock = new Mock<IFileStorageService>();
        var sqsMock = new Mock<ISqsService>();
        var emailMock = new Mock<ISendEmailCommand>();
        var storageMock = new Mock<IStorageService>();
        var loggerMock = new Mock<ILogger<AddVideoCommand>>();

        var command = new AddVideoCommand(
            repoMock.Object, fileStorageMock.Object, sqsMock.Object, emailMock.Object, storageMock.Object, loggerMock.Object);

        var result = await command.Execute(null, "user@email.com");

        Assert.False(result.Success);
        Assert.Equal("Arquivo inválido ou vazio.", result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_ReturnsFailure_WhenFileIsEmpty()
    {
        var file = CreateFakeFormFile(length: 0);

        var repoMock = new Mock<IVideoRepository>();
        var fileStorageMock = new Mock<IFileStorageService>();
        var sqsMock = new Mock<ISqsService>();
        var emailMock = new Mock<ISendEmailCommand>();
        var storageMock = new Mock<IStorageService>();
        var loggerMock = new Mock<ILogger<AddVideoCommand>>();

        var command = new AddVideoCommand(
            repoMock.Object, fileStorageMock.Object, sqsMock.Object, emailMock.Object, storageMock.Object, loggerMock.Object);

        var result = await command.Execute(file, "user@email.com");

        Assert.False(result.Success);
        Assert.Equal("Arquivo inválido ou vazio.", result.ErrorMessage);
    }

    [Fact]
    public async Task Execute_ReturnsFailure_WhenUserIsNullOrEmpty()
    {
        var file = CreateFakeFormFile();

        var repoMock = new Mock<IVideoRepository>();
        var fileStorageMock = new Mock<IFileStorageService>();
        var sqsMock = new Mock<ISqsService>();
        var emailMock = new Mock<ISendEmailCommand>();
        var storageMock = new Mock<IStorageService>();
        var loggerMock = new Mock<ILogger<AddVideoCommand>>();

        var command = new AddVideoCommand(
            repoMock.Object, fileStorageMock.Object, sqsMock.Object, emailMock.Object, storageMock.Object, loggerMock.Object);

        var result1 = await command.Execute(file, null);
        var result2 = await command.Execute(file, "");
        var result3 = await command.Execute(file, "   ");

        Assert.False(result1.Success);
        Assert.Equal("Usuário não pode ser nulo ou vazio.", result1.ErrorMessage);
        Assert.False(result2.Success);
        Assert.Equal("Usuário não pode ser nulo ou vazio.", result2.ErrorMessage);
        Assert.False(result3.Success);
        Assert.Equal("Usuário não pode ser nulo ou vazio.", result3.ErrorMessage);
    }

    [Fact]
    public async Task Execute_ReturnsFailure_AndSendsEmail_WhenSqsThrows()
    {
        // Arrange
        var file = CreateFakeFormFile();
        var usuario = "user@email.com";
        var video = CreateFakeVideo(usuario);

        var repoMock = new Mock<IVideoRepository>();
        repoMock.Setup(r => r.Add(It.IsAny<Video>())).ReturnsAsync(video);
        repoMock.Setup(r => r.Update(It.IsAny<Video>())).ReturnsAsync(video);

        var fileStorageMock = new Mock<IFileStorageService>();
        var sqsMock = new Mock<ISqsService>();
        sqsMock.Setup(s => s.SendAsync(It.IsAny<Video>())).ThrowsAsync(new Exception("SQS error"));

        var emailMock = new Mock<ISendEmailCommand>();
        emailMock.Setup(e => e.SendEmailAsync(
            usuario, video.NomeArquivo, VideoStatus.Erro, "Erro para adicionar video", "SQS error"
        )).Returns(Task.CompletedTask).Verifiable();

        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(s => s.UploadFileAsync(file, It.IsAny<string>())).ReturnsAsync("ok");
        storageMock.Setup(s => s.GetFileUrl(It.IsAny<string>())).Returns("s3://bucket/video.mp4");

        var loggerMock = new Mock<ILogger<AddVideoCommand>>();

        var command = new AddVideoCommand(
            repoMock.Object, fileStorageMock.Object, sqsMock.Object, emailMock.Object, storageMock.Object, loggerMock.Object);

        // Act
        var result = await command.Execute(file, usuario);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Erro ao enviar o vídeo para SQS", result.ErrorMessage);
        emailMock.Verify();
    }

    [Fact]
    public async Task Execute_ReturnsFailure_WhenRepositoryAddThrows()
    {
        var file = CreateFakeFormFile();
        var usuario = "user@email.com";

        var repoMock = new Mock<IVideoRepository>();
        repoMock.Setup(r => r.Add(It.IsAny<Video>())).ThrowsAsync(new Exception("DB error"));

        var fileStorageMock = new Mock<IFileStorageService>();
        var sqsMock = new Mock<ISqsService>();
        var emailMock = new Mock<ISendEmailCommand>();
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(s => s.UploadFileAsync(file, It.IsAny<string>())).ReturnsAsync("ok");
        storageMock.Setup(s => s.GetFileUrl(It.IsAny<string>())).Returns("s3://bucket/video.mp4");

        var loggerMock = new Mock<ILogger<AddVideoCommand>>();

        var command = new AddVideoCommand(
            repoMock.Object, fileStorageMock.Object, sqsMock.Object, emailMock.Object, storageMock.Object, loggerMock.Object);

        var result = await command.Execute(file, usuario);

        Assert.False(result.Success);
        Assert.Equal("Erro ao adicionar o vídeo: ", result.ErrorMessage);
    }
}
