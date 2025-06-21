using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoManager.Domain.Enums;

namespace VideoManager.Application.Common.Reponse
{
    public class VideoResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? VideoId { get; set; }
        public string? NomeArquivo { get; set; }
        public VideoStatus? Status { get; set; }
    }
}
