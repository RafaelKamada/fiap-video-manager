using Microsoft.AspNetCore.Mvc;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common;

namespace VideoManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenCommand token) : ControllerBase
{
    private readonly ITokenCommand _token = token;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = _token.Execute(request);

        if (token.Success)
            return Ok(new { token.Token });
        
        return Unauthorized(new { mensagem = token.MessageError });
    }
}