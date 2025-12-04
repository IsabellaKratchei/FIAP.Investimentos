using FIAP.Application.DTOs.Investments;
using FIAP.Domain.Entities;

public interface IInvestmentService
{
  Task<IEnumerable<Investment>> BuscarTodosAsync(Guid userId);
  Task<Investment> CriarAsync(Guid userId, CriarInvestmentRequest req);
  Task<Investment> AtualizarAsync(Guid userId, Guid investmentId, CriarInvestmentRequest req);
  Task ExcluirAsync(Guid userId, Guid investmentId);
}
