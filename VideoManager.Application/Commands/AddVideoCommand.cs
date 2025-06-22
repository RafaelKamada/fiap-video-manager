using Microsoft.AspNetCore.Http;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Application.Commands;

public class AddVideoCommand(IVideoRepository videoRepository, ISqsService sqsService, ISendEmailCommand sendEmail, IFileStorageService fileStorage) : IAddVideoCommand
{
    private readonly IVideoRepository _videoRepository = videoRepository;
    private readonly IFileStorageService _fileStorage = fileStorage;
    private readonly ISqsService _sqsService = sqsService;
    private readonly ISendEmailCommand _sendEmail = sendEmail;

    public async Task<VideoResult> Execute(IFormFile arquivo, string usuario)
    {
        try
        {
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("Arquivo inválido ou vazio.");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new ArgumentException("Usuário não pode ser nulo ou vazio.");

            //using var memoryStream = new MemoryStream();
            //await arquivo.CopyToAsync(memoryStream);
            //var conteudo = memoryStream.ToArray();
            var caminho = await SaveFile(arquivo);

            Video video = Helper.MapRequest(arquivo, usuario, null, caminho);

            await SaveAsync(video);

            await SendVideoToSqs(video);

            return new VideoResult
            {
                Success = true,
                VideoId = video.Id,
                NomeArquivo = video.NomeArquivo,
                Status = video.Status,
            };
        }
        catch (Exception ex)
        {
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
            throw new Exception("Erro ao adicionar o vídeo", ex);
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
