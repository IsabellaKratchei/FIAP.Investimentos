using FIAP.Domain.Entities;

namespace FIAP.Application.Interfaces
{
  public interface IUsuarioRepository
  {
    // CREATE
    Task AdicionarAsync(Usuario usuario);

    // READ
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task<Usuario?> BuscarPorIdAsync(Guid id);

    // UPDATE
    Task AtualizarAsync(Usuario usuario);

    // PERSISTÊNCIA
    Task SalvarMudancasAsync();
  }
}
