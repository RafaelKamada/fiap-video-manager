using Microsoft.AspNetCore.Mvc;
using VideoManager.Application.Commands;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Enums;
using VideoManager.Domain.Interfaces;

namespace VideoManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController(IVideoRepository videoRepository, IAddVideoCommand addVideoCommand) : ControllerBase
{
    private readonly IVideoRepository _videoRepository = videoRepository;
    private readonly IAddVideoCommand _addVideoCommand = addVideoCommand;

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


    [HttpPut("status/{id}")]
    public async Task<IActionResult> Status(IFormFile arquivo, int id, string usuario)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");
        
        var videoBase = await _videoRepository.Get(id);
        if (videoBase == null || videoBase.Conteudo == null)
            return NotFound();

        using var memoryStream = new MemoryStream();
        await arquivo.CopyToAsync(memoryStream);
        var conteudo = memoryStream.ToArray();

        videoBase.NomeArquivo = arquivo.FileName;
        videoBase.Conteudo = conteudo;
        videoBase.Status = VideoStatus.Uploaded;
        videoBase.Usuario = usuario;

        await _videoRepository.Update(videoBase);

        return Ok(new { videoBase.Id, videoBase.NomeArquivo, videoBase.Status });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var video = await _videoRepository.Get(id);
        if (video == null || video.Conteudo == null)
            return NotFound();

        return File(video.Conteudo, "application/octet-stream", video.NomeArquivo);
    }

    [HttpGet("status/{usuario}")]
    public async Task<IActionResult> ListarPorUsuario(string usuario)
    {
        var videos = await _videoRepository.GetAllByUserId(usuario);
        return Ok(videos.Select(v => new { v.Id, v.NomeArquivo, v.Status, v.DataCriacao }));
    }
}
