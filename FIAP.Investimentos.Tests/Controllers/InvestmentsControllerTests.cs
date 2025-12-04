using FIAP.Application.DTOs.Investments;
using FIAP.Application.Interfaces;
using FIAP.Domain.Entities;
using FIAP.Investimentos.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FIAP.Tests.Controllers
{
  public class InvestmentsControllerTests
  {
    private Mock<IInvestmentService> _serviceMock;
    private InvestmentsController _controller;

    [SetUp]
    public void Setup()
    {
      _serviceMock = new Mock<IInvestmentService>();

      _controller = new InvestmentsController(_serviceMock.Object);

      // Simula usuário autenticado com Claim NameIdentifier
      var userId = Guid.NewGuid().ToString();
      var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

      var identity = new ClaimsIdentity(claims, "TestAuth");
      var principal = new ClaimsPrincipal(identity);

      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = principal }
      };
    }

    [Test]
    public async Task BuscarTodos_DeveRetornarOkComListaDeInvestimentos()
    {
      // Arrange
      var investimentosFake = new List<Investment>
    {
        new Investment(Guid.NewGuid(), "CDB", 100, DateTimeOffset.Now),
        new Investment(Guid.NewGuid(), "Tesouro", 200, DateTimeOffset.Now)
    };

      _serviceMock
          .Setup(s => s.BuscarTodosAsync(It.IsAny<Guid>()))
          .ReturnsAsync(investimentosFake);

      // Act
      var result = await _controller.BuscarTodos();

      // Assert
      var ok = result as OkObjectResult;
      Assert.IsNotNull(ok);
      Assert.AreEqual(200, ok.StatusCode);

      var lista = ok.Value as IEnumerable<Investment>;
      Assert.IsNotNull(lista);
    }

    [Test]
    public async Task Criar_DeveRetornarCreatedComNovoInvestment()
    {
      // Arrange
      var req = new CriarInvestmentRequest
      {
        TipoInvestment = "CDB",
        ValorInvestment = 500,
        DataInvestment = DateTimeOffset.Now
      };

      var investmentCriado = new Investment(Guid.NewGuid(), req.TipoInvestment, req.ValorInvestment, req.DataInvestment);

      _serviceMock
          .Setup(s => s.CriarAsync(It.IsAny<Guid>(), req))
          .ReturnsAsync(investmentCriado);

      // Act
      var result = await _controller.Criar(req);

      // Assert
      var created = result as CreatedAtActionResult;
      Assert.IsNotNull(created);
      Assert.AreEqual(nameof(InvestmentsController.BuscarTodos), created.ActionName);
      Assert.AreEqual(201, created.StatusCode);
    }

    //[Test]
    //public async Task Atualizar_DeveRetornarNoContent()
    //{
    //  // Arrange
    //  var id = Guid.NewGuid();

    //  var req = new CriarInvestmentRequest
    //  {
    //    TipoInvestment = "LCI",
    //    ValorInvestment = 300,
    //    DataInvestment = DateTimeOffset.Now
    //  };

    //  _serviceMock
    //      .Setup(s => s.AtualizarAsync(It.IsAny<Guid>(), id, req))
    //      .Returns(Task.CompletedTask);

    //  // Act
    //  var result = await _controller.Atualizar(id, req);

    //  // Assert
    //  Assert.IsInstanceOf<NoContentResult>(result);
    //}

    [Test]
    public async Task Excluir_DeveRetornarNoContent()
    {
      // Arrange
      var id = Guid.NewGuid();

      _serviceMock
          .Setup(s => s.ExcluirAsync(It.IsAny<Guid>(), id))
          .Returns(Task.CompletedTask);

      // Act
      var result = await _controller.Excluir(id);

      // Assert
      Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public void BuscarTodos_QuandoTokenInvalido_DeveLancarUnauthorizedAccessException()
    {
      // Arrange — remove o usuário autenticado
      _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

      // Act + Assert
      Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
          await _controller.BuscarTodos()
      );
    }
  }
}