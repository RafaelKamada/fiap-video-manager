using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Enums;

namespace VideoManager.Application.Commands.Interfaces;

public interface IUpdateStatusCommand
{
    Task<VideoResult> Execute(int id, string caminho, VideoStatus status);
}
