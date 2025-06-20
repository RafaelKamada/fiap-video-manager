using Microsoft.AspNetCore.Http;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Application.Commands;

public class AddVideoCommand(IVideoRepository videoRepository, ISqsService sqsService) : IAddVideoCommand
{
    private readonly IVideoRepository _videoRepository = videoRepository;
    private readonly ISqsService _sqsService = sqsService;
    public async Task<AddVideoResult> Execute(IFormFile arquivo, string usuario)
    {
        try
        {
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("Arquivo inválido ou vazio.");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new ArgumentException("Usuário não pode ser nulo ou vazio.");

            using var memoryStream = new MemoryStream();
            await arquivo.CopyToAsync(memoryStream);
            var conteudo = memoryStream.ToArray();

            Video video = MapRequest(arquivo, usuario, conteudo);

            await SaveAsync(video);

            await SendVideoToSqs(video);

            return new AddVideoResult
            {
                Success = true,
                VideoId = video.Id,
                NomeArquivo = video.NomeArquivo,
                Status = video.Status,
            };
        }
        catch (Exception ex)
        {
            return new AddVideoResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }            
    }

    private static Video MapRequest(IFormFile arquivo, string usuario, byte[] conteudo)
    {
        return new Video
        {
            NomeArquivo = arquivo.FileName,
            Conteudo = conteudo,
            Status = VideoStatus.Uploaded,
            DataCriacao = DateTime.UtcNow,
            Usuario = usuario
        };
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
            await _videoRepository.Update(video); //Aqui tem que enviar mensagem de erro
            throw new Exception("Erro ao enviar o vídeo para SQS", ex);
        }
    }
}
