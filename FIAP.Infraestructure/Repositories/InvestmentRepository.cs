using FIAP.Application.Interfaces;
using FIAP.Domain.Entities;
using FIAP.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Infraestructure.Repositories
{
  public class InvestmentRepository : IInvestmentRepository
  {
    private readonly AppDbContext _ctx;
    public InvestmentRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Investment> AdicionarInvestmentAsync(Investment Investment)
    {
      _ctx.Investments.Add(Investment);
      await _ctx.SaveChangesAsync();
      return Investment;
    }

    public Task AtualizarInvestmentAsync(Investment Investment)
    {
      _ctx.Investments.Update(Investment);
      return Task.CompletedTask;
    }

    public async Task<List<Investment>> BuscarTodosInvestmentsAsync()
    {
      return await _ctx.Investments.ToListAsync();
    }

    public Task<Investment?> BuscarPorIdInvestmentAsync(Guid id) => _ctx.Investments.FindAsync(id).AsTask();

    public async Task<IEnumerable<Investment>> BuscarPorIdUsuarioAsync(Guid idUsuario) 
    {
      return await _ctx.Investments
      .Where(i => i.IdUsuario == idUsuario)
      .ToListAsync();
    }

    public Task ExcluirInvestmentAsync(Investment Investment)
    {
      _ctx.Investments.Remove(Investment);
      return Task.CompletedTask;
    }

    public Task SalvarMudancasInvestmentAsync()=> _ctx.SaveChangesAsync();
  }
}
