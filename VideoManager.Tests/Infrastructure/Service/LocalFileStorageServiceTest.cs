using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using VideoManager.Infrastructure.Services;

namespace VideoManager.Tests.Infrastructure.Service;

public class LocalFileStorageServiceTest : IDisposable
{
    private readonly string _testDir;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTest()
    {
        // Use a temp directory for tests
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);

        // Use reflection to set the private _pastaVideos field
        _service = new LocalFileStorageService();
        typeof(LocalFileStorageService)
            .GetField("_pastaVideos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_service, _testDir);
    }

    [Fact]
    public async Task SaveAsync_SavesFileAndReturnsPath()
    {
        // Arrange
        var fileName = "test.txt";
        var fileContent = "Hello, world!";
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns<Stream, System.Threading.CancellationToken>((stream, _) => ms.CopyToAsync(stream));

        // Act
        var path = await _service.SaveAsync(fileMock.Object);

        // Assert
        Assert.True(File.Exists(path));
        Assert.Equal(Path.Combine(_testDir, fileName), path);
        var savedContent = await File.ReadAllTextAsync(path);
        Assert.Equal(fileContent, savedContent);
    }

    [Fact]
    public async Task SaveAsync_Throws_OnNullFile()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveAsync(null));
    }

    [Fact]
    public async Task SaveAsync_Throws_OnEmptyFile()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveAsync(fileMock.Object));
    }

    [Fact]
    public async Task DownloadAsync_ReturnsStream_WhenFileExists()
    {
        // Arrange
        var fileName = "download.txt";
        var filePath = Path.Combine(_testDir, fileName);
        var content = "Download me!";
        await File.WriteAllTextAsync(filePath, content);

        // Act
        using var stream = await _service.DownloadAsync(fileName);
        using var reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task DownloadAsync_Throws_WhenFileDoesNotExist()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _service.DownloadAsync("notfound.txt"));
    }

    public void Dispose()
    {
        // Clean up temp directory
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }
}