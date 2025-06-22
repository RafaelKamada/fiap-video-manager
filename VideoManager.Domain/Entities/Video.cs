using VideoManager.Domain.Enums;

namespace VideoManager.Domain.Entities;

public class Video
{
    public int Id { get; set; }
    public string NomeArquivo { get; set; } = null!;
    public byte[]? Conteudo { get; set; }  // Armazena o vídeo ou ZIP
    public VideoStatus Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public string? MensagemErro { get; set; }
    public string Usuario { get; set; } = null!;
    public string? Caminho { get; set; } // Caminho do arquivo no sistema de arquivos
}
 