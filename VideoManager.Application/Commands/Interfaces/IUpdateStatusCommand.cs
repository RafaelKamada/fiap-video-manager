using Microsoft.AspNetCore.Http;
using VideoManager.Application.Common.Reponse;
using VideoManager.Domain.Enums;

namespace VideoManager.Application.Commands.Interfaces;

public interface IUpdateStatusCommand
{
    Task<VideoResult> Execute(IFormFile arquivo, string usuario, int id, string caminho, VideoStatus status);
}
