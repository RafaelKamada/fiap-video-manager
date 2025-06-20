using Microsoft.AspNetCore.Http;
using VideoManager.Application.Common.Reponse;

namespace VideoManager.Application.Commands;

public interface IAddVideoCommand
{
    Task<AddVideoResult> Execute(IFormFile arquivo, string usuario);
}
