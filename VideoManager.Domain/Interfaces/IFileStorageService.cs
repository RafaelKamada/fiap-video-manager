using Microsoft.AspNetCore.Http;

namespace VideoManager.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile arquivo);
    Task<Stream> DownloadAsync(string nomeArquivo);
}
