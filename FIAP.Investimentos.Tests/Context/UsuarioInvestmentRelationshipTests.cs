using System;
using System.Linq;
using System.Threading.Tasks;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FIAP.Tests.Infrastructure
{
  [TestFixture]
  public class UsuarioInvestmentRelationshipTests
  {
    private AppDbContext _context;

    [SetUp]
    public void Setup()
    {
      //var options = new DbContextOptionsBuilder<AppDbContext>()
      //    .UseInMemoryDatabase(Guid.NewGuid().ToString())
      //    .Options;

      //_context = new AppDbContext(options);
      var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Filename=:memory:")
    .Options;

      _context = new AppDbContext(options);
      _context.Database.OpenConnection(); // obrigatório para SQLite in-memory
      _context.Database.EnsureCreated();

    }

    private Usuario CriarUsuario()
    {
      return new Usuario(
          "Isabella",
          "HASH",
          $"isa{Guid.NewGuid()}@teste.com"
      );
    }

    [Test]
    public async Task Usuario_DevePersistirComInvestments()
    {
      var usuario = CriarUsuario();

      var inv = new Investment(
          usuario.Id,
          "Tesouro Direto",
          1500,
          DateTimeOffset.UtcNow
      );

      usuario.Investments.Add(inv);

      await _context.Usuarios.AddAsync(usuario);
      await _context.SaveChangesAsync();

      var encontrado = await _context.Usuarios
          .Include(u => u.Investments)
          .FirstAsync();

      Assert.AreEqual(1, encontrado.Investments.Count);
      Assert.AreEqual("Tesouro Direto", encontrado.Investments.First().TipoInvestment);
    }

    [Test]
    public async Task CarregarUsuario_DeveCarregarInvestments()
    {
      var usuario = CriarUsuario();
      var inv = new Investment(usuario.Id, "Ações", 100, DateTimeOffset.UtcNow);
      usuario.Investments.Add(inv);

      _context.Usuarios.Add(usuario);
      await _context.SaveChangesAsync();

      var loaded = await _context.Usuarios
          .Include(u => u.Investments)
          .FirstAsync(u => u.Id == usuario.Id);

      Assert.NotNull(loaded.Investments);
      Assert.AreEqual(1, loaded.Investments.Count);
    }

    [Test]
    public async Task RemoverUsuario_DeveRemoverInvestments_Cascade()
    {
      var usuario = CriarUsuario();
      var inv = new Investment(usuario.Id, "CDB", 800, DateTimeOffset.UtcNow);

      usuario.Investments.Add(inv);
      _context.Usuarios.Add(usuario);
      await _context.SaveChangesAsync();

      _context.Usuarios.Remove(usuario);
      await _context.SaveChangesAsync();

      Assert.AreEqual(0, _context.Investments.Count());
    }

    [Test]
    public async Task Investment_SemUsuario_DeveFalhar()
    {
      // Cria investimento com usuário inexistente
      var inv = new Investment(
          Guid.NewGuid(), // IdUsuario que não existe no banco
          "CDB",
          1000,
          DateTimeOffset.Now
      );

      await _context.Investments.AddAsync(inv);

      // Ao salvar, deve lançar DbUpdateException
      var ex = Assert.ThrowsAsync<DbUpdateException>(async () =>
      {
        await _context.SaveChangesAsync();
      });

      Assert.IsNotNull(ex);
    }
  }
}
