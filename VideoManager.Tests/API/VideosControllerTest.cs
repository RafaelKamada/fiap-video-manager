using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoManager.API.Controllers;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Tests.API;

public class VideosControllerTest
{
    private readonly Mock<IVideoRepository> _videoRepoMock = new();
    private readonly Mock<IAddVideoCommand> _addVideoCmdMock = new();
    private readonly Mock<IUpdateStatusCommand> _updateStatusCmdMock = new();
    private readonly Mock<IDownloadVideoCommand> _downloadVideoCmdMock = new();

    private VideosController CreateController() =>
        new VideosController(
            _videoRepoMock.Object,
            _addVideoCmdMock.Object,
            _updateStatusCmdMock.Object,
            _downloadVideoCmdMock.Object);

    [Fact]
    public async Task Upload_ReturnsOk_WhenSuccess()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var usuario = "user@email.com";
        var resultObj = new VideoResult { Success = true };

        _addVideoCmdMock.Setup(c => c.Execute(fileMock.Object, usuario))
            .ReturnsAsync(resultObj);

        var controller = CreateController();
        var result = await controller.Upload(fileMock.Object, usuario);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(resultObj, okResult.Value);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenFileIsNull()
    {
        var controller = CreateController();
        var result = await controller.Upload(null, "user@email.com");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Nenhum arquivo enviado.", badRequest.Value);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenFileIsEmpty()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        var controller = CreateController();
        var result = await controller.Upload(fileMock.Object, "user@email.com");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Nenhum arquivo enviado.", badRequest.Value);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenCommandFails()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var usuario = "user@email.com";
        var resultObj = new VideoResult { Success = false, ErrorMessage = "fail" };

        _addVideoCmdMock.Setup(c => c.Execute(fileMock.Object, usuario))
            .ReturnsAsync(resultObj);

        var controller = CreateController();
        var result = await controller.Upload(fileMock.Object, usuario);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(resultObj, badRequest.Value);
    }

    [Fact]
    public async Task Status_ReturnsOk_WhenSuccess()
    {
        var resultObj = new VideoResult { Success = true };
        _updateStatusCmdMock.Setup(c => c.Execute(1, "zip", VideoStatus.Finalizado))
            .ReturnsAsync(resultObj);

        var controller = CreateController();
        var result = await controller.Status(1, "zip", VideoStatus.Finalizado);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(resultObj, okResult.Value);
    }

    [Fact]
    public async Task Status_ReturnsBadRequest_WhenInvalidInput()
    {
        var controller = CreateController();
        var result = await controller.Status(0, "", VideoStatus.Finalizado);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Nenhum arquivo enviado ou caminho do zip inválido.", badRequest.Value);
    }

    [Fact]
    public async Task Status_ReturnsBadRequest_WhenCommandFails()
    {
        var resultObj = new VideoResult { Success = false, ErrorMessage = "fail" };
        _updateStatusCmdMock.Setup(c => c.Execute(1, "zip", VideoStatus.Finalizado))
            .ReturnsAsync(resultObj);

        var controller = CreateController();
        var result = await controller.Status(1, "zip", VideoStatus.Finalizado);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(resultObj, badRequest.Value);
    }

    [Fact]
    public async Task Download_ReturnsFile_WhenSuccess()
    {
        var fileContent = new byte[] { 1, 2, 3 };
        var resultObj = new DownloadVideoResult
        {
            Success = true,
            FileContent = fileContent,
            FileName = "file.zip"
        };
        _downloadVideoCmdMock.Setup(c => c.Execute(1)).ReturnsAsync(resultObj);

        var controller = CreateController();
        var result = await controller.Download(1);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(fileContent, fileResult.FileContents);
        Assert.Equal("application/octet-stream", fileResult.ContentType);
        Assert.Equal("file.zip", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task Download_ReturnsNotFound_WhenNotSuccess()
    {
        var resultObj = new DownloadVideoResult { Success = false, FileContent = null };
        _downloadVideoCmdMock.Setup(c => c.Execute(1)).ReturnsAsync(resultObj);

        var controller = CreateController();
        var result = await controller.Download(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ListarPorUsuario_ReturnsOk_WithVideos()
    {
        var videos = new List<Video>
    {
        new Video { Id = 1, NomeArquivo = "a.mp4", Status = VideoStatus.Uploaded, DataCriacao = System.DateTime.UtcNow },
        new Video { Id = 2, NomeArquivo = "b.mp4", Status = VideoStatus.Finalizado, DataCriacao = System.DateTime.UtcNow }
    };
        _videoRepoMock.Setup(r => r.GetAllByUserId("user")).ReturnsAsync(videos);

        var controller = CreateController();
        var result = await controller.ListarPorUsuario("user");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Equal(2, returned.Count());
    }
}
