using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Infrastructure.Services
{
    public class EmailService(string apiKey, string fromEmail, string fromName) : IEmailService
    {
        private readonly string _apiKey = apiKey;
        private readonly string _fromEmail = fromEmail;
        private readonly string _fromName = fromName;

        public async Task EnviarEmailAsync(string emailDestino, string assunto, string texto, string html = "")
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(emailDestino);
            var msg = MailHelper.CreateSingleEmail(from, to, assunto, texto, html ?? texto);
            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("E-mail enviado com sucesso.");
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"Erro ao enviar e-mail: {responseBody}");
            }
        }
    }
}
