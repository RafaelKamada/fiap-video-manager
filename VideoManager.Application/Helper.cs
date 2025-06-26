using Microsoft.AspNetCore.Http;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;

namespace VideoManager.Application;

public class Helper
{
    public static Video MapRequest(IFormFile arquivo, string usuario, byte[]? conteudo, string caminhoVideoS3)
    {
        return new Video
        {
            NomeArquivo = arquivo.FileName,
            Conteudo = conteudo,
            Status = VideoStatus.Uploaded,
            DataCriacao = DateTime.UtcNow,
            Usuario = usuario,
            CaminhoVideo = caminhoVideoS3
        };
    }
}
