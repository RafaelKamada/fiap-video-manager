using Microsoft.AspNetCore.Http;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _pastaVideos = "/app/videos";

    public async Task<string> SaveAsync(IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            throw new ArgumentException("Arquivo inválido.");

        if (!Directory.Exists(_pastaVideos))
            Directory.CreateDirectory(_pastaVideos);

        var caminhoCompleto = Path.Combine(_pastaVideos, arquivo.FileName);

        using var stream = new FileStream(caminhoCompleto, FileMode.Create);
        await arquivo.CopyToAsync(stream);

        return caminhoCompleto;
    }

    public async Task<Stream> DownloadAsync(string nomeArquivo)
    {
        var caminhoCompleto = Path.Combine(_pastaVideos, nomeArquivo);

        if (!File.Exists(caminhoCompleto))
            throw new FileNotFoundException("Arquivo não encontrado.", nomeArquivo);

        var stream = new FileStream(caminhoCompleto, FileMode.Open, FileAccess.Read);
        return stream;
    }
}
