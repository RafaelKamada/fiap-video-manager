namespace VideoManager.Domain.Entities;

public class VideoContent
{
    public required string Video_Id { get; set; }
    public required string Path { get; set; }
    public required string Extension { get; set; }
}
