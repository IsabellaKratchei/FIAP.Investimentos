using FIAP.Application.Interfaces;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Infraestructure.Repositories
{
  public class UsuarioRepository : IUsuarioRepository
  {
    private readonly AppDbContext _ctx;
    public UsuarioRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<Usuario> BuscarPorEmailAsync(string email) => _ctx.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public async Task AdicionarAsync(Usuario usuario) => await _ctx.Usuarios.AddAsync(usuario);

    public Task<Usuario> BuscarPorIdAsync(Guid id) => _ctx.Usuarios.FindAsync(id).AsTask();

    public Task SalvarMudancasAsync()=> _ctx.SaveChangesAsync();

    public Task AtualizarAsync(Usuario usuario)
    {
      _ctx.Usuarios.Update(usuario);
      return Task.CompletedTask;
    }
  }
}
