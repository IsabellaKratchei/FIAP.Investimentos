using FIAP.Domain.Entities;

namespace FIAP.Application.Interfaces
{
  public interface IInvestmentRepository
  {
    Task<IEnumerable<Investment>> BuscarPorIdUsuarioAsync(Guid idUsuario);
    Task<Investment?> BuscarPorIdInvestmentAsync(Guid id);
    Task<Investment> AdicionarInvestmentAsync(Investment Investment);
    Task AtualizarInvestmentAsync(Investment Investment);
    Task ExcluirInvestmentAsync(Investment Investment);
    Task SalvarMudancasInvestmentAsync();
  }
}
