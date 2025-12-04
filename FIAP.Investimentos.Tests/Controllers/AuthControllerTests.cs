using System;
using System.Threading.Tasks;
using FIAP.Application.DTOs.Auth;
using FIAP.Application.Interfaces;
using FIAP.Application.Services;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using FIAP.Infraestructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using FIAP.Investimentos.Controllers;

namespace FIAP.Tests.Controllers
{
  [TestFixture]
  public class AuthControllerTests
  {
    private AppDbContext _context;
    private UsuarioRepository _usuarioRepo;
    private Mock<ISenhaHasher> _hashMock;
    private Mock<IJwtTokenService> _jwtMock;
    private AuthenticationService _authService;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new AppDbContext(options);
      _usuarioRepo = new UsuarioRepository(_context);
      _hashMock = new Mock<ISenhaHasher>();
      _jwtMock = new Mock<IJwtTokenService>();

      _authService = new AuthenticationService(
          _usuarioRepo,
          _hashMock.Object,
          _jwtMock.Object
      );

      _controller = new AuthController(_authService);
    }

    //[Test]
    //public async Task Registrar_DeveRetornar201_QuandoDadosValidos()
    //{
    //  var req = new RegisterRequest("Isabella", "isa@teste.com", "123");
    //  _hashMock.Setup(h => h.Hash(req.Senha)).Returns("HASH");

    //  var result = await _controller.Register(req);

    //  Assert.IsInstanceOf<CreatedResult>(result);
    //}

    [Test]
    public async Task Registrar_DeveRetornar400_QuandoEmailDuplicado()
    {
      var usuario = new Usuario("Isa", "HASH", "isa@teste.com");
      await _usuarioRepo.AdicionarAsync(usuario);
      await _usuarioRepo.SalvarMudancasAsync();

      var req = new RegisterRequest("Outra", "isa@teste.com", "123");
      var result = await _controller.Register(req) as BadRequestObjectResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(400, result.StatusCode);
    }

    //[Test]
    //public async Task Login_DeveRetornarToken_QuandoCredenciaisValidas()
    //{
    //  var usuario = new Usuario("Isa", "HASH", "isa@teste.com");
    //  await _usuarioRepo.AdicionarAsync(usuario);
    //  await _usuarioRepo.SalvarMudancasAsync();

    //  _hashMock.Setup(h => h.VerificaHash("123", "HASH")).Returns(true);
    //  _jwtMock.Setup(j => j.GenerateToken(usuario)).Returns("TOKEN123");

    //  var req = new LoginRequest("isa@teste.com", "123");

    //  var result = await _controller.Login(req) as OkObjectResult;

    //  Assert.IsNotNull(result);
    //  Assert.AreEqual(200, result.StatusCode);

    //  var resp = result.Value as AuthResponse;
    //  Assert.AreEqual("TOKEN123", resp.Token);
    //}

    [Test]
    public async Task Login_DeveRetornar401_QuandoEmailNaoEncontrado()
    {
      var req = new LoginRequest("naoexiste@teste.com", "123");

      var result = await _controller.Login(req);

      Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
    }

    [Test]
    public async Task Login_DeveRetornar401_QuandoSenhaInvalida()
    {
      var usuario = new Usuario("Isa", "HASH", "isa@teste.com");
      await _usuarioRepo.AdicionarAsync(usuario);
      await _usuarioRepo.SalvarMudancasAsync();

      _hashMock.Setup(h => h.VerificaHash("123", "HASH")).Returns(false);

      var req = new LoginRequest("isa@teste.com", "123");

      var result = await _controller.Login(req);

      Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
    }
  }
}
