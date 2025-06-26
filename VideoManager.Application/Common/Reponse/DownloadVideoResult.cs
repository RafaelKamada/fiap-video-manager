namespace VideoManager.Application.Common.Reponse
{
    public class DownloadVideoResult
    {
        public bool Success { get; set; }
        public byte[]? FileContent { get; set; }
        public string? FileName { get; set; }
    }
}
