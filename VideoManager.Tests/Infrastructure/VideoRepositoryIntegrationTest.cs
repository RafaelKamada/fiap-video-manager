using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Infrastructure.Repositories;

namespace VideoManager.Tests.Infrastructure;

public class VideoRepositoryIntegrationTest : IAsyncLifetime
{
    private readonly string _connectionString;
    private VideoRepository _repository;

    public VideoRepositoryIntegrationTest()
    {
        // Load connection string from configuration or environment variable
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Test database connection string not found.");

        _repository = new VideoRepository(_connectionString);
    }

    public async Task InitializeAsync()
    {
        // Ensure the Videos table exists
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Videos (
                Id SERIAL PRIMARY KEY,
                NomeArquivo TEXT,
                Conteudo BYTEA,
                CaminhoVideo TEXT,
                CaminhoZip TEXT,
                Status INT,
                DataCriacao TIMESTAMP,
                Usuario TEXT,
                MensagemErro TEXT
            );
        ");
        await conn.CloseAsync();
    }

    public Task DisposeAsync()
    {
        // Optionally clean up data between tests
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Execute("DELETE FROM Videos;");
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Add_And_Get_Video_Works()
    {
        var video = new Video
        {
            NomeArquivo = "test.mp4",
            Conteudo = new byte[] { 1, 2, 3 },
            CaminhoVideo = "s3://test.mp4",
            Status = VideoStatus.Uploaded,
            DataCriacao = DateTime.UtcNow,
            Usuario = "user@email.com",
            MensagemErro = null
        };

        var added = await _repository.Add(video);
        Assert.True(added.Id > 0);

        var fetched = await _repository.Get(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal("test.mp4", fetched.NomeArquivo);
        Assert.Equal("user@email.com", fetched.Usuario);
    }

    [Fact]
    public async Task Update_Video_Works()
    {
        var video = new Video
        {
            NomeArquivo = "test2.mp4",
            Conteudo = new byte[] { 4, 5, 6 },
            CaminhoVideo = "s3://test2.mp4",
            Status = VideoStatus.Uploaded,
            DataCriacao = DateTime.UtcNow,
            Usuario = "user2@email.com",
            MensagemErro = null
        };

        var added = await _repository.Add(video);
        added.Status = VideoStatus.Finalizado;
        added.CaminhoZip = "s3://test2.zip";
        added.MensagemErro = "none";

        var updated = await _repository.Update(added);

        Assert.Equal(VideoStatus.Finalizado, updated.Status);
        Assert.Equal("s3://test2.zip", updated.CaminhoZip);
        Assert.Equal("none", updated.MensagemErro);
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsVideos()
    {
        var video1 = new Video
        {
            NomeArquivo = "a.mp4",
            Conteudo = new byte[] { 1 },
            CaminhoVideo = "s3://a.mp4",
            Status = VideoStatus.Uploaded,
            DataCriacao = DateTime.UtcNow,
            Usuario = "user3@email.com",
            MensagemErro = null
        };
        var video2 = new Video
        {
            NomeArquivo = "b.mp4",
            Conteudo = new byte[] { 2 },
            CaminhoVideo = "s3://b.mp4",
            Status = VideoStatus.Uploaded,
            DataCriacao = DateTime.UtcNow,
            Usuario = "user3@email.com",
            MensagemErro = null
        };

        await _repository.Add(video1);
        await _repository.Add(video2);

        var videos = (await _repository.GetAllByUserId("user3@email.com")).ToList();
        Assert.NotNull(videos);
        Assert.True(videos.Count >= 2);
    }
}
