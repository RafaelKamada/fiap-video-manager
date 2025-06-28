using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController(IVideoRepository videoRepository, 
                                IAddVideoCommand addVideoCommand, 
                                IUpdateStatusCommand updateStatusCommand,
                                IDownloadVideoCommand downloadVideoCommand) : ControllerBase
{
    private readonly IVideoRepository _videoRepository = videoRepository;
    private readonly IAddVideoCommand _addVideoCommand = addVideoCommand;
    private readonly IUpdateStatusCommand _updateStatusCommand = updateStatusCommand;
    private readonly IDownloadVideoCommand _downloadVideoCommand = downloadVideoCommand;

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile arquivo, [FromQuery] string usuario)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        var result = await _addVideoCommand.Execute(arquivo, usuario);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [Authorize]
    [HttpPut("status/{id}")]
    public async Task<IActionResult> Status(int id, string caminhoZip, VideoStatus status)
    {
        if (id <= 0 || string.IsNullOrEmpty(caminhoZip))
            return BadRequest("Nenhum arquivo enviado ou caminho do zip inválido.");

        var result = await _updateStatusCommand.Execute(id, caminhoZip, status);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await _downloadVideoCommand.Execute(id);

        if (!result.Success || result?.FileContent == null)
            return NotFound();

        return File(result.FileContent, "application/octet-stream", result.FileName);
    }

    [Authorize]
    [HttpGet("status/{usuario}")]
    public async Task<IActionResult> ListarPorUsuario(string usuario)
    {
        var videos = await _videoRepository.GetAllByUserId(usuario);
        return Ok(videos.Select(v => new { v.Id, v.NomeArquivo, v.Status, v.DataCriacao }));
    }
}
