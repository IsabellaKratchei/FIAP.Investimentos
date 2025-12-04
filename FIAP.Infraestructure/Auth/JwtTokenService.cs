using FIAP.Domain.Entities;
using FIAP.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FIAP.Infraestructure.Auth
{
  public class JwtTokenService : IJwtTokenService
  {
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public string GenerateToken(Usuario usuario)
    {
      if (usuario == null) return null;

      var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
      var issuer = _configuration["Jwt:Issuer"];
      var audience = _configuration["Jwt:Audience"];

      var claims = new[]
      {
        new Claim("Id", usuario.Id.ToString()),
        new Claim("Email", usuario.Email ?? string.Empty),
        new Claim("Nome", usuario.Nome ?? string.Empty)
      };

      var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
