using Microsoft.AspNetCore.Http;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace VideoManager.Application.Commands;

public class AddVideoCommand : IAddVideoCommand
{
    private readonly IVideoRepository _videoRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly ISqsService _sqsService;
    private readonly ISendEmailCommand _sendEmail;
    private readonly IStorageService _storageService;
    private readonly ILogger<AddVideoCommand> _logger;

    public AddVideoCommand(
        IVideoRepository videoRepository,
        IFileStorageService fileStorage,
        ISqsService sqsService,
        ISendEmailCommand sendEmail,
        IStorageService storageService,
        ILogger<AddVideoCommand> logger)
    {
        _videoRepository = videoRepository;
        _fileStorage = fileStorage;
        _sqsService = sqsService;
        _sendEmail = sendEmail;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<VideoResult> Execute(IFormFile arquivo, string usuario)
    {
        try
        {
            if (arquivo == null || arquivo.Length == 0)
                return new VideoResult { Success = false, ErrorMessage = "Arquivo inválido ou vazio." };

            if (string.IsNullOrWhiteSpace(usuario))
                return new VideoResult { Success = false, ErrorMessage = "Usuário não pode ser nulo ou vazio." };

            // Gera um nome único para o arquivo
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(arquivo.FileName)}";
            
            // Faz upload do arquivo para o S3
            await _storageService.UploadFileAsync(arquivo, fileName);
            var caminhoS3 = _storageService.GetFileUrl(fileName);

            Video video = Helper.MapRequest(arquivo, usuario, null, caminhoS3);

            await SaveAsync(video);

            await SendVideoToSqs(video);

            return new VideoResult
            {
                Success = true,
                VideoId = video.Id,
                NomeArquivo = video.NomeArquivo,
                Status = video.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar upload do vídeo");
            return new VideoResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }            
    }

    private async Task<string> SaveFile(IFormFile file)
    {
        try
        {
            Console.WriteLine($"Salvando arquivo: {file.FileName}");

            return await _fileStorage.SaveAsync(file);
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao salvar o arquivo", ex);
        }
    }
    private async Task SaveAsync(Video video)
    {
        try
        {
            Console.WriteLine($"Adicionando vídeo: {video.NomeArquivo}, Status: {video.Status}, Usuário: {video.Usuario}");
            await _videoRepository.Add(video);
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao adicionar o vídeo: " + video?.MensagemErro, ex);
        }
    }

    private async Task SendVideoToSqs(Video video)
    {
        try
        {
            Console.WriteLine($"Enviando vídeo {video.NomeArquivo} para SQS...");
            await _sqsService.SendAsync(video);
        }
        catch (Exception ex)
        {
            video.Status = VideoStatus.Erro;
            video.MensagemErro = ex.Message;

            await _videoRepository.Update(video);

            await SendEmail(video, ex.Message);
            
            Console.WriteLine($"Erro ao enviar o vídeo para SQS: {ex.Message}");

            throw new Exception("Erro ao enviar o vídeo para SQS", ex);
        }
    }

    private async Task SendEmail(Video video, string mensagemErro)
    {
        try
        {
            Console.WriteLine($"Enviando e-mail para o usuário: {video.Usuario}, Assunto: Erro ao adicionar vídeo");

            await _sendEmail.SendEmailAsync(video.Usuario, video.NomeArquivo, video.Status, "Erro para adicionar video", mensagemErro);
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao enviar e-mail", ex);
        }
    }
}
