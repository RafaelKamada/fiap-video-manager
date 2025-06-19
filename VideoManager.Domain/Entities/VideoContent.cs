namespace VideoManager.Domain.Entities;

public class VideoContent
{
    public required string VideoId { get; set; }
    public required string Content { get; set; }
    public required string Extension { get; set; }
}
