using VideoManager.Domain.Enums;

namespace VideoManager.Domain.Entities;

public class Video
{
    public int Id { get; set; }
    public string NomeArquivo { get; set; } = null!;
    public byte[]? Conteudo { get; set; }  // Armazena o vídeo ou ZIP
    public string? CaminhoVideo { get; set; }  // caminho S3
    public VideoStatus Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public string? MensagemErro { get; set; }
    public string Usuario { get; set; } = null!;
    public string? CaminhoZip { get; set; } // Caminho do zip no S3
}
 