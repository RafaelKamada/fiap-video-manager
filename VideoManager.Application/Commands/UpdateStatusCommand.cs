using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;
using VideoManager.Infrastructure.Repositories;
namespace VideoManager.Application.Commands;

public class UpdateStatusCommand(IVideoRepository videoRepository, ISendEmailCommand sendEmailCommand) : IUpdateStatusCommand
{
    private readonly IVideoRepository _videoRepository = videoRepository;
    private readonly ISendEmailCommand _sendEmailCommand = sendEmailCommand;

    public async Task<VideoResult> Execute(IFormFile arquivo, string usuario, int id)
    {
        try
        {
            var videoBase = await _videoRepository.Get(id);

            if (videoBase == null || videoBase.Conteudo == null)
                throw new Exception("Vídeo não encontrado ou sem conteúdo.");

            using var memoryStream = new MemoryStream();
            await arquivo.CopyToAsync(memoryStream);
            var conteudo = memoryStream.ToArray();

            if (string.IsNullOrWhiteSpace(usuario))
                throw new ArgumentException("Usuário não pode ser nulo ou vazio.");

            var request = Helper.MapRequest(arquivo, usuario, conteudo);
            request.Id = videoBase.Id;

            await _videoRepository.Update(videoBase);

            return new VideoResult
            {
                Success = true,
                VideoId = request.Id,
                NomeArquivo = request.NomeArquivo,
                Status = request.Status,
            };
        }
        catch (Exception ex)
        {
            SendEmail(id, usuario, "Erro ao atualizar vídeo", ex.Message).GetAwaiter().GetResult();

            return new VideoResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task SendEmail(int id, string usuario, string assunto, string mensagemErro)
    {
        try
        {
            Console.WriteLine($"Enviando e-mail para o usuário: {usuario}, Assunto: {assunto}");

            await _sendEmailCommand.SendEmailAsync(usuario, id.ToString(), VideoStatus.Erro, assunto, mensagemErro);
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao enviar e-mail", ex);
        }
    }
}
