using Microsoft.AspNetCore.Http;

namespace VideoManager.Domain.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string fileName);
    string GetFileUrl(string fileName); 
    Task<MemoryStream> DownloadFileAsync(string fileKey);
}
