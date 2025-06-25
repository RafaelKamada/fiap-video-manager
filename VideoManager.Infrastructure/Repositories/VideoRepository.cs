using Dapper;
using Npgsql;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Infrastructure.Repositories;

public class VideoRepository(string connectionString) : IVideoRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<Video?> Get(int id)
    {
        Console.WriteLine("id: " + id);
        Console.WriteLine("connectionString: " + _connectionString);
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Video>(
            "SELECT * FROM Videos WHERE Id = @Id", 
            new { Id = id });
    }

    public async Task<IEnumerable<Video>> GetAllByUserId(string usuario)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Video>(
            "SELECT * FROM Videos WHERE Usuario = @Usuario ORDER BY DataCriacao DESC", 
            new { Usuario = usuario });
    }

    public async Task<Video> Add(Video video)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var id = await connection.QuerySingleAsync<int>(
            @"INSERT INTO Videos (NomeArquivo, Conteudo, Caminho, Status, DataCriacao, Usuario, MensagemErro) 
              VALUES (@NomeArquivo, @Conteudo, @Caminho, @Status, @DataCriacao, @Usuario, @MensagemErro)
              RETURNING Id",
            video);

        video.Id = id;
        return video;
    }

    public async Task<Video> Update(Video video)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE Videos 
              SET NomeArquivo = @NomeArquivo,
                  Conteudo = @Conteudo,
                  Caminho = @Caminho,
                  Status = @Status,
                  MensagemErro = @MensagemErro
              WHERE Id = @Id",
            video);

        return video;
    }
}