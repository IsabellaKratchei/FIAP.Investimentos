using System;
using System.Threading.Tasks;
using FIAP.Application.DTOs.Auth;
using FIAP.Application.Interfaces;
using FIAP.Application.Services;
using FIAP.Domain.Entities;
using Moq;
using NUnit.Framework;

[TestFixture]
public class JwtTokenServiceTests
{
  private Mock<IUsuarioRepository> _usuarioRepoMock;
  private Mock<ISenhaHasher> _senhaHasherMock;
  private Mock<IJwtTokenService> _jwtMock;

  private AuthenticationService _service;

  [SetUp]
  public void Setup()
  {
    _usuarioRepoMock = new Mock<IUsuarioRepository>();
    _senhaHasherMock = new Mock<ISenhaHasher>();
    _jwtMock = new Mock<IJwtTokenService>();

    _service = new AuthenticationService(
        _usuarioRepoMock.Object,
        _senhaHasherMock.Object,
        _jwtMock.Object
    );
  }

  // =============================================================
  // LOGIN
  // =============================================================

  [Test]
  public async Task LoginAsync_DeveRetornarToken_QuandoCredenciaisValidas()
  {
    // Arrange
    var req = new LoginRequest("teste@fiap.com", "123");

    var usuarioFake = new Usuario("Teste", "teste@fiap.com", "HASH-SENHA");
    var tokenFake = "TOKEN_JWT";

    _usuarioRepoMock
        .Setup(r => r.BuscarPorEmailAsync(req.Email))
        .ReturnsAsync(usuarioFake);

    _senhaHasherMock
        .Setup(h => h.VerificaHash(req.Senha, usuarioFake.SenhaHash))
        .Returns(true);

    _jwtMock
        .Setup(j => j.GenerateToken(usuarioFake))
        .Returns(tokenFake);

    // Act
    var result = await _service.LoginAsync(req);

    // Assert
    Assert.AreEqual(usuarioFake.Nome, result.Nome);
    Assert.AreEqual(usuarioFake.Email, result.Email);
    Assert.AreEqual(tokenFake, result.Token);
  }

  [Test]
  public void LoginAsync_DeveLancarUnauthorized_QuandoEmailNaoExiste()
  {
    // Arrange
    var req = new LoginRequest("naoexiste@fiap.com", "123");

    _usuarioRepoMock
        .Setup(r => r.BuscarPorEmailAsync(req.Email))
        .ReturnsAsync((Usuario)null);

    // Act + Assert
    Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(req));
  }

  [Test]
  public void LoginAsync_DeveLancarUnauthorized_QuandoSenhaInvalida()
  {
    // Arrange
    var req = new LoginRequest("teste@fiap.com", "senhaerrada");

    var usuarioFake = new Usuario("Teste", "teste@fiap.com", "HASH");

    _usuarioRepoMock
        .Setup(r => r.BuscarPorEmailAsync(req.Email))
        .ReturnsAsync(usuarioFake);

    _senhaHasherMock
        .Setup(h => h.VerificaHash(req.Senha, usuarioFake.SenhaHash))
        .Returns(false);

    // Act & Assert
    Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(req));
  }

  // =============================================================
  // REGISTER
  // =============================================================

  [Test]
  public async Task RegistrarAsync_DeveCriarUsuarioERetornarAuthResponse()
  {
    // Arrange
    var req = new RegisterRequest("Joao", "joao@fiap.com", "123456");

    var hashFake = "HASH-GERADO";
    var usuarioCriado = new Usuario(req.Nome, req.Email, hashFake);
    var tokenFake = "TOKEN123";

    _usuarioRepoMock
        .Setup(r => r.BuscarPorEmailAsync(req.Email))
        .ReturnsAsync((Usuario)null); // email não existe

    _senhaHasherMock
        .Setup(h => h.Hash(req.Senha))
        .Returns(hashFake);

    _usuarioRepoMock
        .Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
        .Returns(Task.CompletedTask);

    _usuarioRepoMock
        .Setup(r => r.SalvarMudancasAsync())
        .Returns(Task.CompletedTask);

    _jwtMock
        .Setup(j => j.GenerateToken(It.IsAny<Usuario>()))
        .Returns(tokenFake);

    // Act
    var result = await _service.RegistrarAsync(req);

    // Assert
    Assert.AreEqual(req.Nome, result.Nome);
    Assert.AreEqual(req.Email, result.Email);
    Assert.AreEqual(tokenFake, result.Token);
  }


  [Test]
  public void RegistrarAsync_DeveLancarInvalidOperation_QuandoEmailJaExiste()
  {
    // Arrange
    var req = new RegisterRequest("Ana", "ana@fiap.com", "123");

    var usuarioExistente = new Usuario("Ana", "ana@fiap.com", "HASH");

    _usuarioRepoMock
        .Setup(r => r.BuscarPorEmailAsync(req.Email))
        .ReturnsAsync(usuarioExistente);

    // Act + Assert
    Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegistrarAsync(req));
  }
}
