using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using FIAP.Infraestructure.Auth;
using FIAP.Domain.Entities;
using System.Security.Claims;

namespace FIAP.Investimentos.Tests.Infrastructure.Auth
{
  [TestFixture]
  public class BCryptSenhaHasherTests
  {
    private BCryptSenhaHasher _hasher;

    [SetUp]
    public void Setup()
    {
      _hasher = new BCryptSenhaHasher();
    }

    [Test]
    public void GerarHash_DeveRetornarValorDiferenteDaSenha()
    {
      var senha = "123456";
      var hash = _hasher.Hash(senha);

      Assert.AreNotEqual(senha, hash);
      Assert.IsTrue(hash.Length > 10);
    }

    [Test]
    public void Verificar_DeveRetornarTrue_ParaSenhaCorreta()
    {
      var senha = "abc123";
      var hash = _hasher.Hash(senha);

      Assert.IsTrue(_hasher.VerificaHash(senha, hash));
    }

    [Test]
    public void Verificar_DeveRetornarFalse_ParaSenhaErrada()
    {
      var hash = _hasher.Hash("senha boa");

      Assert.False(_hasher.VerificaHash("senha errada", hash));
    }
  }
}
