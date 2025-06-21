using Microsoft.AspNetCore.Http;
using VideoManager.Application.Common.Reponse;

namespace VideoManager.Application.Commands.Interfaces;

public interface IAddVideoCommand
{
    Task<VideoResult> Execute(IFormFile arquivo, string usuario);
}
