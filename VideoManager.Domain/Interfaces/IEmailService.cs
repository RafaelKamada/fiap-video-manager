using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoManager.Domain.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string emailDestino, string assunto, string texto, string html = null);
    }
}
