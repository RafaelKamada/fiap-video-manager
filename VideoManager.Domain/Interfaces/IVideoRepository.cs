using VideoManager.Domain.Entities;

namespace VideoManager.Domain.Interfaces;

public interface IVideoRepository
{
    Task<Video?> Get(int id);
    Task<IEnumerable<Video>> GetAllByUserId(string usuario);
    Task<Video> Add(Video video);
    Task<Video> Update(Video video);
}