using System;
using System.Linq;
using System.Threading.Tasks;
using FIAP.Application.DTOs.Auth;
using FIAP.Application.Services;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using FIAP.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FIAP.Investimentos.Tests.Infrastructure
{
  [TestFixture]
  public class UsuarioRepositoryTests
  {
    private AppDbContext _context = null!;
    private UsuarioRepository _repo = null!;
    private AuthenticationService _authService = null!;

    [SetUp]
    public void Setup()
    {
      //var options = new DbContextOptionsBuilder<AppDbContext>()
      //    .UseInMemoryDatabase(Guid.NewGuid().ToString())
      //    .Options;

      //_context = new AppDbContext(options);
      // Configura SQLite in-memory
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseSqlite("Filename=:memory:") // banco real em memória
          .Options;

      _context = new AppDbContext(options);
      _context.Database.OpenConnection(); // necessário para SQLite in-memory
      _context.Database.EnsureCreated();

      _repo = new UsuarioRepository(_context);
      //_repo = new UsuarioRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
      _context.Dispose();
    }

    // ============================================================================
    // UTIL
    // ============================================================================
    private Usuario CriarUsuario(string email = "user@teste.com")
    {
      var user = new Usuario("Isabella", "HASH123", email)
      {
        Id = Guid.NewGuid()
      };
      return user;
    }

    // ============================================================================
    // TESTE 1 — ADICIONAR
    // ============================================================================
    [Test]
    public async Task AdicionarAsync_DeveAdicionarUsuarioNoBanco()
    {
      var usuario = CriarUsuario();

      await _repo.AdicionarAsync(usuario);
      await _repo.SalvarMudancasAsync();

      var salvo = await _context.Usuarios.FindAsync(usuario.Id);

      Assert.IsNotNull(salvo);
      Assert.AreEqual(usuario.Email, salvo!.Email);
      Assert.AreEqual(usuario.Nome, salvo.Nome);
    }

    // ============================================================================
    // TESTE 2 — BUSCAR POR EMAIL
    // ============================================================================
    [Test]
    public async Task ObterPorEmailAsync_DeveRetornarUsuarioQuandoExistir()
    {
      var usuario = CriarUsuario();
      _context.Usuarios.Add(usuario);
      await _context.SaveChangesAsync();

      var encontrado = await _repo.BuscarPorEmailAsync(usuario.Email);

      Assert.IsNotNull(encontrado);
      Assert.AreEqual(usuario.Email, encontrado!.Email);
    }

    [Test]
    public async Task ObterPorEmailAsync_DeveRetornarNullQuandoNaoExistir()
    {
      var encontrado = await _repo.BuscarPorEmailAsync("inexistente@teste.com");

      Assert.IsNull(encontrado);
    }

    // ============================================================================
    // TESTE 3 — BUSCAR POR ID
    // ============================================================================
    [Test]
    public async Task ObterPorIdAsync_DeveRetornarUsuarioQuandoExistir()
    {
      var usuario = CriarUsuario();
      _context.Usuarios.Add(usuario);
      await _context.SaveChangesAsync();

      var encontrado = await _repo.BuscarPorIdAsync(usuario.Id);

      Assert.IsNotNull(encontrado);
      Assert.AreEqual(usuario.Id, encontrado!.Id);
    }

    [Test]
    public async Task ObterPorIdAsync_DeveRetornarNullQuandoNaoExistir()
    {
      var encontrado = await _repo.BuscarPorIdAsync(Guid.NewGuid());
      Assert.IsNull(encontrado);
    }

    // ============================================================================
    // TESTE 4 — EVITAR EMAIL DUPLICADO
    // ============================================================================
    [Test]
    public async Task AdicionarAsync_NaoDeveAdicionarQuandoEmailDuplicado()
    {
      var usuario1 = CriarUsuario("duplicado@teste.com");
      var usuario2 = CriarUsuario("duplicado@teste.com");

      await _repo.AdicionarAsync(usuario1);
      await _repo.SalvarMudancasAsync();

      // Tenta adicionar usuário duplicado
      var ex = Assert.ThrowsAsync<DbUpdateException>(async () =>
      {
        await _repo.AdicionarAsync(usuario2);
        await _repo.SalvarMudancasAsync();
      });

      Assert.IsNotNull(ex);
    }

    // ============================================================================
    // TESTE 5 — ATUALIZAR SENHA
    // ============================================================================
    [Test]
    public async Task AtualizarSenhaAsync_DevePersistirNovaSenha()
    {
      var usuario = CriarUsuario();
      await _repo.AdicionarAsync(usuario);
      await _repo.SalvarMudancasAsync();

      usuario.AtualizarSenha("NOVO_HASH");

      await _repo.SalvarMudancasAsync();

      var atualizado = await _context.Usuarios.FindAsync(usuario.Id);

      Assert.IsNotNull(atualizado);
      Assert.AreEqual("NOVO_HASH", atualizado!.SenhaHash);
    }

    // ============================================================================
    // TESTE 6 — ATUALIZAR ENTIDADE COMPLETA
    // ============================================================================
    [Test]
    public async Task AtualizarAsync_DeveAtualizarTodosOsCampos()
    {
      var usuario = CriarUsuario();
      await _repo.AdicionarAsync(usuario);
      await _repo.SalvarMudancasAsync();

      usuario.Nome = "Novo Nome";
      usuario.Email = "novo@teste.com";

      await _repo.AtualizarAsync(usuario);
      await _repo.SalvarMudancasAsync();

      var atualizado = await _context.Usuarios.FindAsync(usuario.Id);

      Assert.AreEqual("Novo Nome", atualizado!.Nome);
      Assert.AreEqual("novo@teste.com", atualizado.Email);
    }

    // ============================================================================
    // TESTE 7 — INCLUIR INVESTMENTS NA CONSULTA (caso o repo inclua Include)
    // ============================================================================
    [Test]
    public async Task ObterPorIdAsync_DeveRetornarUsuarioComInvestments()
    {
      var usuario = CriarUsuario();

      var inv = new Investment(usuario.Id, "Ações", 100, DateTimeOffset.UtcNow);

      _context.Usuarios.Add(usuario);
      _context.Investments.Add(inv);
      await _context.SaveChangesAsync();

      var encontrado = await _repo.BuscarPorIdAsync(usuario.Id);

      Assert.IsNotNull(encontrado);
      Assert.IsNotNull(encontrado!.Investments);
      Assert.AreEqual(1, encontrado.Investments.Count);
    }


    // ============================================================================
    // TESTE 8 — SALVAR MUDANÇAS
    // ============================================================================
    [Test]
    public async Task SalvarMudancasAsync_DeveRealizarPersistenciaGeral()
    {
      var usuario = CriarUsuario();

      await _repo.AdicionarAsync(usuario);
      await _repo.SalvarMudancasAsync();

      var salvo = await _context.Usuarios.FindAsync(usuario.Id);

      Assert.IsNotNull(salvo);
    }
  }
}
