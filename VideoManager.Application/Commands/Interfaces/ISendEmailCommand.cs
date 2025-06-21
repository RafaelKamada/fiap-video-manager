using VideoManager.Domain.Enums;

namespace VideoManager.Application.Commands.Interfaces;

public interface ISendEmailCommand
{
    public Task SendEmailAsync(string usuario, string nomeArquivo, VideoStatus status, string assunto, string mensagem);
}
