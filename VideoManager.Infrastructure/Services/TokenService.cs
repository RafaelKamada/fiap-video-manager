using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Infrastructure.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    private readonly IConfiguration _config = config;

    public string GerarToken(string email)
    {
        var chave = Encoding.ASCII.GetBytes(_config["JwtSettings:Key"]);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Name, email)
        }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:ExpiryMinutes"])),
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(chave),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
