using FIAP.Application.DTOs.Investments;
using FIAP.Application.Interfaces;
using FIAP.Application.Services;
using FIAP.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace FIAP.Tests.Services
{
  public class InvestmentServiceTests
  {
    private Mock<IInvestmentRepository> _repoMock = null!;
    private InvestmentService _service = null!;

    [SetUp]
    public void Setup()
    {
      _repoMock = new Mock<IInvestmentRepository>();
      _service = new InvestmentService(_repoMock.Object);
    }

    [Test]
    public async Task BuscarTodosAsync_DeveRetornarInvestmentsDoUsuario()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var lista = new List<Investment>
    {
        new Investment(userId, "Renda Fixa", 1000, DateTimeOffset.Now),
        new Investment(userId, "Ações", 500, DateTimeOffset.Now)
    };

      _repoMock
          .Setup(r => r.BuscarPorIdUsuarioAsync(userId))
          .ReturnsAsync(lista);

      // Act
      var result = await _service.BuscarTodosAsync(userId);

      // Assert
      Assert.AreEqual(2, result.Count());
    }

    [Test]
    public async Task CriarAsync_DeveCriarInvestmentERetornarObjeto()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var req = new CriarInvestmentRequest
      {
        TipoInvestment = "Tesouro Direto",
        ValorInvestment = 1200,
        DataInvestment = DateTimeOffset.Now
      };

      _repoMock
    .Setup(r => r.AdicionarInvestmentAsync(It.IsAny<Investment>()))
    .ReturnsAsync((Investment inv) => inv);

      _repoMock
          .Setup(r => r.SalvarMudancasInvestmentAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _service.CriarAsync(userId, req);

      // Assert
      Assert.AreEqual(req.TipoInvestment, result.TipoInvestment);
      Assert.AreEqual(req.ValorInvestment, result.ValorInvestment);
      Assert.AreEqual(userId, result.IdUsuario);
    }

    [Test]
    public void AtualizarAsync_QuandoInvestmentNaoExiste_DeveLancarExcecao()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var invId = Guid.NewGuid();

      _repoMock
          .Setup(r => r.BuscarPorIdInvestmentAsync(invId))
          .ReturnsAsync((Investment?)null);

      var req = new CriarInvestmentRequest
      {
        TipoInvestment = "CDB",
        ValorInvestment = 2000,
        DataInvestment = DateTimeOffset.Now
      };

      // Act & Assert
      Assert.ThrowsAsync<KeyNotFoundException>(() =>
          _service.AtualizarAsync(userId, invId, req));
    }

    [Test]
    public async Task AtualizarAsync_QuandoInvestmentNaoPertenceAoUsuario_DeveLancarExcecao()
    {
      var usuarioId = Guid.NewGuid();
      var outroUsuarioId = Guid.NewGuid();
      var investimento = new Investment(outroUsuarioId, "Ação", 100, DateTimeOffset.UtcNow);

      _repoMock.Setup(r => r.BuscarPorIdInvestmentAsync(investimento.Id))
               .ReturnsAsync(investimento);

      var request = new CriarInvestmentRequest { TipoInvestment = "Ação", ValorInvestment = 200, DataInvestment = DateTimeOffset.UtcNow };

      // Ajuste aqui para UnauthorizedAccessException
      Assert.ThrowsAsync<UnauthorizedAccessException>(
          async () => await _service.AtualizarAsync(usuarioId, investimento.Id, request)
      );
    }

    [Test]
    public async Task AtualizarAsync_DeveAtualizarInvestment()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var invId = Guid.NewGuid();

      var investimento = new Investment(userId, "Ações", 300, DateTimeOffset.Now);

      _repoMock
          .Setup(r => r.BuscarPorIdInvestmentAsync(invId))
          .ReturnsAsync(investimento);

      _repoMock
          .Setup(r => r.AtualizarInvestmentAsync(investimento))
          .Returns(Task.CompletedTask);

      _repoMock
          .Setup(r => r.SalvarMudancasInvestmentAsync())
          .Returns(Task.CompletedTask);

      var req = new CriarInvestmentRequest
      {
        TipoInvestment = "CDB",
        ValorInvestment = 5000,
        DataInvestment = DateTimeOffset.Now
      };

      // Act
      await _service.AtualizarAsync(userId, invId, req);

      // Assert
      Assert.AreEqual(req.TipoInvestment, investimento.TipoInvestment);
      Assert.AreEqual(req.ValorInvestment, investimento.ValorInvestment);
    }

    [Test]
    public void ExcluirAsync_QuandoInvestmentNaoExiste_DeveLancarExcecao()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var invId = Guid.NewGuid();

      _repoMock
          .Setup(r => r.BuscarPorIdInvestmentAsync(invId))
          .ReturnsAsync((Investment?)null);

      // Act & Assert
      Assert.ThrowsAsync<KeyNotFoundException>(() =>
          _service.ExcluirAsync(userId, invId));
    }

    [Test]
    public async Task ExcluirAsync_DeveRemoverInvestment()
    {
      // Arrange
      var userId = Guid.NewGuid();
      var invId = Guid.NewGuid();

      var investimento = new Investment(userId, "Ações", 300, DateTimeOffset.Now);

      _repoMock
          .Setup(r => r.BuscarPorIdInvestmentAsync(invId))
          .ReturnsAsync(investimento);

      _repoMock
          .Setup(r => r.ExcluirInvestmentAsync(investimento))
          .Returns(Task.CompletedTask);

      _repoMock
          .Setup(r => r.SalvarMudancasInvestmentAsync())
          .Returns(Task.CompletedTask);

      // Act
      await _service.ExcluirAsync(userId, invId);

      // Assert
      _repoMock.Verify(r => r.ExcluirInvestmentAsync(investimento), Times.Once);
    }
  }
}
