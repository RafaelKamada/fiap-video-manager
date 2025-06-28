using Moq;
using System.Text;
using VideoManager.Application.Commands;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Tests.Application
{
    public class DownloadVideoCommandTest
    {
        [Fact]
        public async Task Execute_ReturnsSuccess_WhenVideoAndZipExist()
        {
            // Arrange
            var video = new VideoManager.Domain.Entities.Video
            {
                Id = 1,
                NomeArquivo = "video.mp4",
                CaminhoZip = "folder/video.zip"
            };
            var fileContent = Encoding.UTF8.GetBytes("zipcontent");
            var fileStream = new MemoryStream(fileContent);

            var videoRepoMock = new Mock<IVideoRepository>();
            videoRepoMock.Setup(r => r.Get(1)).ReturnsAsync(video);

            var storageMock = new Mock<IStorageService>();
            storageMock.Setup(s => s.DownloadFileAsync("video.zip")).ReturnsAsync(fileStream);

            var command = new DownloadVideoCommand(videoRepoMock.Object, storageMock.Object);

            // Act
            var result = await command.Execute(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(fileContent, result.FileContent);
            Assert.Equal("video.zip", result.FileName);
        }

        [Fact]
        public async Task Execute_ReturnsFailure_WhenVideoNotFound()
        {
            var videoRepoMock = new Mock<IVideoRepository>();
            videoRepoMock.Setup(r => r.Get(It.IsAny<int>())).ReturnsAsync((VideoManager.Domain.Entities.Video)null);

            var storageMock = new Mock<IStorageService>();
            var command = new DownloadVideoCommand(videoRepoMock.Object, storageMock.Object);

            var result = await command.Execute(1);

            Assert.False(result.Success);
            Assert.Null(result.FileContent);
            Assert.Null(result.FileName);
        }

        [Fact]
        public async Task Execute_ReturnsFailure_WhenCaminhoZipIsNullOrEmpty()
        {
            var video = new VideoManager.Domain.Entities.Video
            {
                Id = 1,
                NomeArquivo = "video.mp4",
                CaminhoZip = null
            };
            var videoRepoMock = new Mock<IVideoRepository>();
            videoRepoMock.Setup(r => r.Get(1)).ReturnsAsync(video);

            var storageMock = new Mock<IStorageService>();
            var command = new DownloadVideoCommand(videoRepoMock.Object, storageMock.Object);

            var result = await command.Execute(1);

            Assert.False(result.Success);
            Assert.Null(result.FileContent);
            Assert.Null(result.FileName);
        }

        [Fact]
        public async Task Execute_ReturnsFailure_OnException()
        {
            var videoRepoMock = new Mock<IVideoRepository>();
            videoRepoMock.Setup(r => r.Get(It.IsAny<int>())).ThrowsAsync(new Exception("DB error"));

            var storageMock = new Mock<IStorageService>();
            var command = new DownloadVideoCommand(videoRepoMock.Object, storageMock.Object);

            var result = await command.Execute(1);

            Assert.False(result.Success);
            Assert.Null(result.FileContent);
            Assert.Null(result.FileName);
        }
    }
}