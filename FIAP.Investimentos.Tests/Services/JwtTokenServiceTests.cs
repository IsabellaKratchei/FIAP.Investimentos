using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using FIAP.Infraestructure.Auth;
using FIAP.Domain.Entities;
using System.Security.Claims;
using FIAP.Application.Interfaces;

namespace FIAP.Investimentos.Tests.Services
{
  [TestFixture]
  public class JwtTokenServiceTests
  {
    private JwtTokenService _service;

    [SetUp]
    public void Setup()
    {
      var inMemorySettings = new[]
      {
                new KeyValuePair<string,string>("Jwt:Key","chave-super-secreta-para-testes-1234567890123456"),
                new KeyValuePair<string,string>("Jwt:Issuer","FIAP.Investimentos.Test"),
                new KeyValuePair<string,string>("Jwt:Audience","FIAP.Investimentos.Client")
            };

      IConfiguration config = new ConfigurationBuilder()
          .AddInMemoryCollection(inMemorySettings)
          .Build();

      _service = new JwtTokenService(config);
    }

    //[Test]
    //public void GerarToken_DeveRetornarTokenValidoComClaimsCorretos()
    //{
    //  // Arrange
    //  var usuario = new Usuario("Isabella", "HASH123", "teste@email.com")
    //  {
    //    Id = Guid.NewGuid()
    //  };

    //  // Act
    //  var token = _service.GenerateToken(usuario);

    //  var handler = new JwtSecurityTokenHandler();
    //  var jwt = handler.ReadJwtToken(token);

    //  // Assert
    //  Assert.IsNotNull(jwt);

    //  // Verifica se as claims existem
    //  var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == "Id");
    //  var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == "Email");
    //  var nomeClaim = jwt.Claims.FirstOrDefault(c => c.Type == "Nome");

    //  Assert.IsNotNull(idClaim, "Claim Id não encontrada no token.");
    //  Assert.AreEqual(usuario.Id.ToString(), idClaim!.Value);

    //  Assert.IsNotNull(emailClaim, "Claim Email não encontrada no token.");
    //  Assert.AreEqual(usuario.Email, emailClaim!.Value);

    //  Assert.IsNotNull(nomeClaim, "Claim Nome não encontrada no token.");
    //  Assert.AreEqual(usuario.Nome, nomeClaim!.Value);
    //}
  }
}
