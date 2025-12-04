using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FIAP.Application.DTOs.Investments;
using FIAP.Application.Services;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using FIAP.Infraestructure.Repositories;
using FIAP.Investimentos.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FIAP.Investimentos.Tests.Integration
{
  [TestFixture]
  public class InvestmentsIntegrationTests
  {
    private AppDbContext _context;
    private InvestmentRepository _repo;
    private InvestmentService _service;
    private InvestmentsController _controller;
    private Guid _fakeUserId;

    [SetUp]
    public void Setup()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new AppDbContext(options);
      _repo = new InvestmentRepository(_context);
      _service = new InvestmentService(_repo);
      _controller = new InvestmentsController(_service);

      // 🔥 Simula usuário autenticado
      _fakeUserId = Guid.NewGuid();

      var claims = new[]
      {
                new Claim(ClaimTypes.NameIdentifier, _fakeUserId.ToString())
            };

      _controller.ControllerContext = new ControllerContext()
      {
        HttpContext = new DefaultHttpContext()
        {
          User = new ClaimsPrincipal(
                  new ClaimsIdentity(claims, "TestAuth"))
        }
      };
    }

    //// ===============================================
    //// 1. TESTE: Criar investimento
    //// ===============================================
    //[Test]
    //public async Task Criar_DevePersistirEVoltar201()
    //{
    //  // 1️ Simula o usuário logado
    //  var usuarioLogadoId = Guid.NewGuid();

    //  _controller.ControllerContext = new ControllerContext
    //  {
    //    HttpContext = new DefaultHttpContext
    //    {
    //      User = new ClaimsPrincipal(
    //              new ClaimsIdentity(new Claim[]
    //              {
    //                new Claim("id", usuarioLogadoId.ToString())
    //              }, "TestAuth") // Nome do esquema fictício para teste
    //          )
    //    }
    //  };

    //  // 2️ Cria a request de criação do investimento
    //  var request = new CriarInvestmentRequest
    //  {
    //    TipoInvestment = "CDB",
    //    ValorInvestment = 500,
    //    DataInvestment = DateTimeOffset.Now
    //  };

    //  // 3️ Chama o método Criar do controller
    //  var result = await _controller.Criar(request);

    //  // 4️ Valida se retornou CreatedAtActionResult (201)
    //  Assert.IsInstanceOf<CreatedAtActionResult>(result);

    //  var createdResult = result as CreatedAtActionResult;
    //  Assert.IsNotNull(createdResult);

    //  // 5️ Valida se os dados retornados estão corretos
    //  var investimentoRetornado = createdResult!.Value as InvestmentResponse;
    //  Assert.IsNotNull(investimentoRetornado);
    //  Assert.AreEqual(request.TipoInvestment, investimentoRetornado!.TipoInvestment);
    //  Assert.AreEqual(request.ValorInvestment, investimentoRetornado.ValorInvestment);

    //  // 6️ Opcional: valida se persistiu no banco
    //  var salvoNoBanco = await _context.Investments.FindAsync(investimentoRetornado.Id);
    //  Assert.IsNotNull(salvoNoBanco);
    //  Assert.AreEqual(request.TipoInvestment, salvoNoBanco!.TipoInvestment);
    //}

    //// ===============================================
    //// 2. TESTE: Buscar todos (somente os do usuário)
    //// ===============================================
    //[Test]
    //public async Task BuscarTodos_DeveRetornarSomenteInvestimentosDoUsuario()
    //{
    //  // 1️⃣ Id do usuário logado no teste
    //  var usuarioLogadoId = Guid.NewGuid();

    //  // 2️⃣ Investment do usuário logado
    //  var inv = new Investment(
    //      usuarioLogadoId,
    //      "CDB",
    //      500,
    //      DateTimeOffset.Now
    //  );
    //  _context.Investments.Add(inv);

    //  // 3️⃣ Investment de outro usuário
    //  var invOutro = new Investment(
    //      Guid.NewGuid(),
    //      "LCI",
    //      900,
    //      DateTimeOffset.Now
    //  );
    //  _context.Investments.Add(invOutro);

    //  await _context.SaveChangesAsync();

    //  // 4️⃣ Mock do usuário logado no controller (simula autenticação)
    //  _controller.ControllerContext = new ControllerContext
    //  {
    //    HttpContext = new DefaultHttpContext
    //    {
    //      User = new ClaimsPrincipal(
    //              new ClaimsIdentity(new Claim[]
    //              {
    //                new Claim("id", usuarioLogadoId.ToString())
    //              }, "TestAuth")
    //          )
    //    }
    //  };

    //  // 5️⃣ Chama o método BuscarTodos do controller
    //  var result = await _controller.BuscarTodos();

    //  // 6️⃣ Verifica se retornou OkObjectResult
    //  Assert.IsInstanceOf<OkObjectResult>(result);

    //  var okResult = result as OkObjectResult;
    //  Assert.IsNotNull(okResult);

    //  // 7️⃣ Valida os investimentos retornados
    //  var lista = okResult!.Value as IEnumerable<InvestmentResponse>;
    //  Assert.IsNotNull(lista);
    //  Assert.AreEqual(1, lista!.Count());
    //  Assert.AreEqual("CDB", lista.First().TipoInvestment);
    //  Assert.AreEqual(500, lista.First().ValorInvestment);

    //  // 8️⃣ Opcional: garante que o investimento de outro usuário não está presente
    //  Assert.IsFalse(lista.Any(i => i.TipoInvestment == "LCI"));
    //}


    // ===============================================
    // 3. TESTE: Atualizar investimento
    // ===============================================
    [Test]
    public async Task Atualizar_DevePersistirAlteracoesERetornar204()
    {
      var inv = new Investment(
          _fakeUserId,
          "CDB",
          500,
          DateTimeOffset.Now
      );
      _context.Investments.Add(inv);
      await _context.SaveChangesAsync();

      var req = new CriarInvestmentRequest
      {
        TipoInvestment = "Tesouro IPCA",
        ValorInvestment = 1500,
        DataInvestment = DateTimeOffset.Now
      };

      var result = await _controller.Atualizar(inv.Id, req);

      Assert.IsInstanceOf<NoContentResult>(result);

      var db = await _context.Investments.FindAsync(inv.Id);
      Assert.NotNull(db);
      Assert.AreEqual("Tesouro IPCA", db.TipoInvestment);
      Assert.AreEqual(1500, db.ValorInvestment);
    }

    // ===============================================
    // 4. TESTE: Excluir
    // ===============================================
    [Test]
    public async Task Excluir_DeveRemoverERetornar204()
    {
      var inv = new Investment(
          _fakeUserId,
          "FII",
          800,
          DateTimeOffset.Now
      );

      _context.Investments.Add(inv);
      await _context.SaveChangesAsync();

      var result = await _controller.Excluir(inv.Id);
      Assert.IsInstanceOf<NoContentResult>(result);

      var exists = await _context.Investments.AnyAsync(x => x.Id == inv.Id);
      Assert.False(exists);
    }

    // ===============================================
    // 5. TESTE: Impedir acesso a investimentos de outro usuário
    // ===============================================
    [Test]
    public async Task Atualizar_DeveGerarExcecao_QuandoInvestimentoNaoPertenceAoUsuario()
    {
      // Id do usuário logado no teste
      var usuarioLogadoId = Guid.NewGuid();

      // Cria um investimento que pertence a outro usuário
      var outroUsuarioId = Guid.NewGuid();
      var investimento = new Investment(outroUsuarioId, "CDB", 500, DateTimeOffset.Now);

      _context.Investments.Add(investimento);
      await _context.SaveChangesAsync();

      // Request de atualização
      var request = new CriarInvestmentRequest
      {
        TipoInvestment = "LCI",
        ValorInvestment = 1000,
        DataInvestment = DateTimeOffset.Now
      };

      // Verifica se lança UnauthorizedAccessException
      var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
      {
        await _service.AtualizarAsync(usuarioLogadoId, investimento.Id, request);
      });

      Assert.AreEqual("Investimento não pertence ao usuário.", ex.Message);
    }

    // ===============================================
    // 6. TESTE: Criar deve falhar se request inválido
    // ===============================================
    [Test]
    public async Task Criar_DeveRetornarBadRequest_SeModelInvalido()
    {
      var request = new CriarInvestmentRequest
      {
        TipoInvestment = null, // ou string.Empty
        ValorInvestment = 500,
        DataInvestment = DateTimeOffset.Now
      };

      // Simula ModelState inválido
      _controller.ModelState.AddModelError("TipoInvestment", "Tipo é obrigatório.");

      var result = await _controller.Criar(request);

      Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }
  }
}
