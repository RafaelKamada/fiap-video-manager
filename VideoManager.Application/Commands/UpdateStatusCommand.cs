using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;
namespace VideoManager.Application.Commands;

public class UpdateStatusCommand : IUpdateStatusCommand
{
    private readonly IVideoRepository _videoRepository;
    private readonly ISendEmailCommand _sendEmailCommand;
    private readonly IStorageService _storageService;

    public UpdateStatusCommand(IVideoRepository videoRepository, ISendEmailCommand sendEmailCommand, IStorageService storageService)
    {
        _videoRepository = videoRepository;
        _sendEmailCommand = sendEmailCommand;
        _storageService = storageService;
    }

    public async Task<VideoResult> Execute(int id, string caminho, VideoStatus status)
    {
        string usuario = string.Empty;

        try
        {
            var videoBase = await _videoRepository.Get(id);

            if (videoBase == null || videoBase.CaminhoVideo == null)
                return new VideoResult { Success = false, ErrorMessage = "Vídeo não encontrado ou sem conteúdo." };
                        
            usuario = videoBase.Usuario;
            videoBase.Status = status; 
            videoBase.CaminhoZip = caminho;

            await _videoRepository.Update(videoBase);

            return new VideoResult
            {
                Success = true,
                VideoId = videoBase.Id,
                NomeArquivo = videoBase.NomeArquivo,
                Status = videoBase.Status,
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
