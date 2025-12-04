using System;
using System.Linq;
using System.Threading.Tasks;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using FIAP.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FIAP.Tests.Infrastructure
{
  [TestFixture]
  public class InvestmentRepositoryTests
  {
    private AppDbContext _context = null!;
    private InvestmentRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      _context = new AppDbContext(options);
      _repository = new InvestmentRepository(_context);
    }

    private Investment CreateInvestment()
    {
      return new Investment(
          Guid.NewGuid(),
          "Ações",
          1000m,
          DateTimeOffset.UtcNow
      );
    }

    // -------------------------------
    // TESTE: Add
    // -------------------------------
    [Test]
    public async Task AddAsync_DeveAdicionarInvestmentComSucesso()
    {
      var investment = CreateInvestment();

      var result = await _repository.AdicionarInvestmentAsync(investment);

      Assert.NotNull(result);
      Assert.AreEqual(investment.Id, result.Id);
      Assert.AreEqual(1, _context.Investments.Count());
    }

    // -------------------------------
    // TESTE: GetById
    // -------------------------------
    [Test]
    public async Task GetByIdAsync_DeveRetornarInvestment()
    {
      var investment = CreateInvestment();
      _context.Investments.Add(investment);
      await _context.SaveChangesAsync();

      var result = await _repository.BuscarPorIdInvestmentAsync(investment.Id);

      Assert.NotNull(result);
      Assert.AreEqual(investment.Id, result!.Id);
    }

    [Test]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
      var result = await _repository.BuscarPorIdInvestmentAsync(Guid.NewGuid());

      Assert.IsNull(result);
    }

    // -------------------------------
    // TESTE: GetAll
    // -------------------------------
    [Test]
    public async Task GetAllAsync_DeveRetornarTodosInvestments()
    {
      _context.Investments.Add(CreateInvestment());
      _context.Investments.Add(CreateInvestment());
      await _context.SaveChangesAsync();

      var result = await _repository.BuscarTodosInvestmentsAsync();

      Assert.AreEqual(2, result.Count);
    }

    // -------------------------------
    // TESTE: Update
    // -------------------------------
    [Test]
    public async Task UpdateAsync_DeveAtualizarInvestment()
    {
      var investment = CreateInvestment();
      _context.Investments.Add(investment);
      await _context.SaveChangesAsync();

      investment.AtualizaInvestment("Tesouro Direto", 2500m, DateTimeOffset.UtcNow);

      await _repository.AtualizarInvestmentAsync(investment);

      var atualizado = await _context.Investments.FindAsync(investment.Id);

      Assert.NotNull(atualizado);
      Assert.AreEqual("Tesouro Direto", atualizado!.TipoInvestment);
      Assert.AreEqual(2500m, atualizado.ValorInvestment);
    }

    // -------------------------------
    // TESTE: Delete
    // -------------------------------
    [Test]
    public async Task DeleteAsync_DeveRemoverInvestment()
    {
      // Arrange
      var investimento = new Investment(Guid.NewGuid(), "CDB", 1000, DateTimeOffset.Now);
      await _repository.AdicionarInvestmentAsync(investimento);
      await _repository.SalvarMudancasInvestmentAsync();

      // Act
      await _repository.ExcluirInvestmentAsync(investimento);
      await _repository.SalvarMudancasInvestmentAsync(); // <-- importante

      // Assert
      var todos = await _context.Investments.ToListAsync();
      Assert.AreEqual(0, todos.Count);
    }

  }
}
