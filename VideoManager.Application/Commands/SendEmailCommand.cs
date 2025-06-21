using VideoManager.Application.Commands.Interfaces;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Application.Commands;

public class SendEmailCommand(IEmailService emailService) : ISendEmailCommand
{
    private readonly IEmailService _emailService = emailService;

    public async Task SendEmailAsync(string usuario, string nomeArquivo, VideoStatus status, string assunto, string mensagem)
    {
        try
        {
            Console.WriteLine($"Enviando e-mail para o usuário: {usuario}, Assunto: {assunto}");

            var template = TemplateError(nomeArquivo, status.ToString(), mensagem);

            await _emailService.EnviarEmailAsync(usuario, assunto, mensagem, template);
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar e-mail: {ex.Message} - {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Template de erro do e-mail para notificar sobre problemas no processamento de vídeos.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private static string TemplateError(string item, string status, string mensagem)
    {
        return $@"
                    <!DOCTYPE html>
                    <html lang='pt-BR'>
                    <head>
                      <meta charset='UTF-8'>
                      <title>Fiap Video Manager - Notificação de Erro</title>
                      <style>
                        body {{
                          background-color: #f4f4f4;
                          margin: 0;
                          padding: 0;
                          font-family: Arial, sans-serif;
                        }}
                        .container {{
                          background-color: #ffffff;
                          max-width: 600px;
                          margin: 30px auto;
                          padding: 20px;
                          border-radius: 8px;
                          box-shadow: 0 2px 5px rgba(0,0,0,0.1);
                        }}
                        .header {{
                          background-color: #e74c3c;
                          padding: 10px 20px;
                          border-radius: 8px 8px 0 0;
                          color: white;
                          text-align: center;
                        }}
                        .content {{
                          padding: 20px;
                          color: #333333;
                        }}
                        .content h2 {{
                          color: #e74c3c;
                        }}
                        .footer {{
                          margin-top: 30px;
                          font-size: 12px;
                          color: #999999;
                          text-align: center;
                        }}
                        .button {{
                          display: inline-block;
                          padding: 10px 20px;
                          margin-top: 20px;
                          background-color: #e74c3c;
                          color: white;
                          text-decoration: none;
                          border-radius: 4px;
                        }}
                        .button:hover {{
                          background-color: #c0392b;
                        }}
                      </style>
                    </head>
                    <body>
                      <div class='container'>
                        <div class='header'>
                          <h1>⚠️ Fiap Video Manager - Notificação de Erro</h1>
                        </div>
                        <div class='content'>
                          <h2>Erro identificado no processamento</h2>
                          <p>Olá,</p>
                          <p>Informamos que ocorreu um erro ao processar o seguinte item:</p>

                          <p><strong>Item:</strong> <span style='color: #e74c3c;'>{item}</span></p>
                          <p><strong>Status:</strong> {status}</p>
                          <p><strong>Motivo:</strong> {mensagem}</p>

                          <p>Por favor, verifique e tome as ações necessárias.</p>

                          <p>Se você acha que isso foi um engano, entre em contato com o suporte.</p>
                        </div>
                        <div class='footer'>
                          <p>&copy; 2025 Fiap Video Manager - Todos os direitos reservados.</p>
                        </div>
                      </div>
                    </body>
                    </html>";
    }
}
