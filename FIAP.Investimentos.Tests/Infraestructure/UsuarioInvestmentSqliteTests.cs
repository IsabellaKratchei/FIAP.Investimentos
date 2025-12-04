using System;
using System.Linq;
using System.Threading.Tasks;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FIAP.Investimentos.Tests.Infrastructure
{
  [TestFixture]
  public class UsuarioInvestmentSqliteTests
  {
    private SqliteConnection _connection = null!;
    private DbContextOptions<AppDbContext> _options = null!;

    [SetUp]
    public void Setup()
    {
      _connection = new SqliteConnection("DataSource=:memory:");
      _connection.Open();

      _options = new DbContextOptionsBuilder<AppDbContext>()
          .UseSqlite(_connection)
          .Options;

      // Create schema
      using var ctx = new AppDbContext(_options);
      ctx.Database.EnsureDeleted();
      ctx.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
      _connection.Close();
      _connection.Dispose();
    }

    private Usuario CriarUsuario()
    {
      var u = new Usuario("Isabella", "HASH", $"isa{Guid.NewGuid()}@teste.com");
      u.Id = Guid.NewGuid();
      return u;
    }

    [Test]
    public async Task Usuario_DevePersistirComInvestments_AndInclude_Works()
    {
      using var ctx = new AppDbContext(_options);

      var usuario = CriarUsuario();
      var inv = new Investment(usuario.Id, "Tesouro Direto", 1500, DateTimeOffset.UtcNow);

      usuario.Investments.Add(inv);
      ctx.Usuarios.Add(usuario);
      await ctx.SaveChangesAsync();

      var loaded = await ctx.Usuarios.Include(u => u.Investments).FirstAsync(u => u.Id == usuario.Id);

      Assert.AreEqual(1, loaded.Investments.Count);
      Assert.AreEqual("Tesouro Direto", loaded.Investments.First().TipoInvestment);
    }

    [Test]
    public async Task RemoverUsuario_DeveRemoverInvestments_Cascade()
    {
      using var ctx = new AppDbContext(_options);

      var usuario = CriarUsuario();
      var inv = new Investment(usuario.Id, "CDB", 800, DateTimeOffset.UtcNow);

      usuario.Investments.Add(inv);
      ctx.Usuarios.Add(usuario);
      await ctx.SaveChangesAsync();

      ctx.Usuarios.Remove(usuario);
      await ctx.SaveChangesAsync();

      var count = await ctx.Investments.CountAsync();
      Assert.AreEqual(0, count);
    }

    [Test]
    public void InserirInvestmentComUsuarioIdVazio_DeveFalharPorFK()
    {
      using var ctx = new AppDbContext(_options);

      var inv = new Investment(Guid.Empty, "XYZ", 999, DateTimeOffset.UtcNow);
      ctx.Investments.Add(inv);

      // Save should throw because FK constraint will fail on SQLite (if configured)
      Assert.ThrowsAsync<DbUpdateException>(async () => await ctx.SaveChangesAsync());
    }
  }
}
