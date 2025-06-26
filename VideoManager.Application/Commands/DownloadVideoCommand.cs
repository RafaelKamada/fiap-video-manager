using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Application.Commands
{
    public class DownloadVideoCommand : IDownloadVideoCommand
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IStorageService _storageService;

        public DownloadVideoCommand(IVideoRepository videoRepository, IStorageService storageService)
        {
            _videoRepository = videoRepository;
            _storageService = storageService;
        }

        public async Task<DownloadVideoResult> Execute(int videoId)
        {
            try
            {
                var video = await _videoRepository.Get(videoId);
                if (video == null || string.IsNullOrEmpty(video.CaminhoZip))
                    return new DownloadVideoResult { Success = false };

                var fileKey = video.CaminhoZip.Split('/').Last();
                using var fileStream = await _storageService.DownloadFileAsync(fileKey);

                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                return new DownloadVideoResult
                {
                    Success = true,
                    FileContent = fileBytes,
                    FileName = Path.GetFileNameWithoutExtension(video.NomeArquivo) + ".zip"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Falha no download do arquivo: " + videoId  + " - Erro: " + ex.Message);
                return new DownloadVideoResult { Success = false };
            }
        }
    }
}
