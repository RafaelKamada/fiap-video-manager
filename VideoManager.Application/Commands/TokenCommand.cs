using Microsoft.Extensions.Configuration;
using VideoManager.Application.Commands.Interfaces;
using VideoManager.Application.Common.Reponse;
using VideoManager.Application.Common;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Application.Commands;

public class TokenCommand(IConfiguration config, ITokenService tokenService) : ITokenCommand
{
    private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

    public LoginResponse Execute(LoginRequest request)
    {
        if (ValidateUser(request) && ValidatePassword(request))
        {
            var token = _tokenService.GerarToken(request.User);
            return new LoginResponse { Success = true, Token = token };
        }

        return new LoginResponse { Success = false, MessageError = "Login ou senha inválidos" };
    }

    private bool ValidatePassword(LoginRequest request)
    {
        string hash = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim());
        
        Console.WriteLine("Senha: " + request.Password.Trim());
        Console.WriteLine("Hash BCrypt: " + hash);

        var senhaHash = _config["Auth:Password"];
        return BCrypt.Net.BCrypt.Verify(request.Password.Trim(), senhaHash);
    }

    private bool ValidateUser(LoginRequest request)
    {
        var user = _config["Auth:User"];
        return string.Equals(request.User, user, StringComparison.OrdinalIgnoreCase);
    }
}
