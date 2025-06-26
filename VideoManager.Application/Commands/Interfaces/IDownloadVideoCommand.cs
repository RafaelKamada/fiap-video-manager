
using VideoManager.Application.Common.Reponse;

namespace VideoManager.Application.Commands.Interfaces
{
    public interface IDownloadVideoCommand
    {
        Task<DownloadVideoResult> Execute(int videoId);
    }
}
