using FIAP.Application.DTOs.Investments;
using FIAP.Application.Interfaces;
using FIAP.Domain.Entities;

public class InvestmentService : IInvestmentService
{
  private readonly IInvestmentRepository _repo;

  public InvestmentService(IInvestmentRepository repo)
  {
    _repo = repo;
  }

  public async Task<IEnumerable<Investment>> BuscarTodosAsync(Guid userId)
  {
    return await _repo.BuscarPorIdUsuarioAsync(userId);
  }

  public async Task<Investment> CriarAsync(Guid userId, CriarInvestmentRequest req)
  {
    if (req == null)
      throw new ArgumentNullException(nameof(req));

    if (string.IsNullOrWhiteSpace(req.TipoInvestment))
      throw new ArgumentException("Tipo é obrigatório.");

    if (req.ValorInvestment <= 0)
      throw new ArgumentException("O valor deve ser maior que zero.");

    var inv = new Investment(
        userId,
        req.TipoInvestment,
        req.ValorInvestment,
        req.DataInvestment
    );

    await _repo.AdicionarInvestmentAsync(inv);
    await _repo.SalvarMudancasInvestmentAsync();

    return inv;
  }

  public async Task<Investment> AtualizarAsync(Guid usuarioId,Guid investimentoId, CriarInvestmentRequest request)
  {
    // Busca o investimento no repositório
    var investimento = await _repo.BuscarPorIdInvestmentAsync(investimentoId);
    if (investimento == null)
      throw new KeyNotFoundException("Investimento não encontrado.");

    // Valida se o investimento pertence ao usuário
    if (investimento.IdUsuario != usuarioId)
      throw new UnauthorizedAccessException("Investimento não pertence ao usuário.");

    // Atualiza os valores usando o método da entidade
    investimento.AtualizaInvestment(
        request.TipoInvestment,
        request.ValorInvestment,
        request.DataInvestment
    );

    // Persiste as mudanças
    await _repo.SalvarMudancasInvestmentAsync();

    return investimento;
  }

  public async Task ExcluirAsync(Guid userId, Guid investmentId)
  {
    var inv = await _repo.BuscarPorIdInvestmentAsync(investmentId);

    if (inv == null)
      throw new KeyNotFoundException("Investment não encontrado.");

    if (inv.IdUsuario != userId)
      throw new UnauthorizedAccessException("Usuário não autorizado.");

    await _repo.ExcluirInvestmentAsync(inv);
    await _repo.SalvarMudancasInvestmentAsync();
  }
}
