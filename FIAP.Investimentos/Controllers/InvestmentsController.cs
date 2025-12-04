using FIAP.Application.DTOs.Investments;
using FIAP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FIAP.Investimentos.Controllers
{
  [ApiController]
  [Route("investments")]
  [Authorize] // Todos os endpoints requerem autenticação
  public class InvestmentsController : ControllerBase
  {
    private readonly IInvestmentService _service;

    public InvestmentsController(IInvestmentService service)
    {
      _service = service;
    }

    // =========================
    // Método auxiliar para obter o Id do usuário logado
    // =========================
    private Guid ObterUsuarioIdDoToken()
    {
      // Se você usou o .Clear() no Program.cs, a claim será exatamente "Id"
      var idClaim = User.FindFirstValue("Id");

      if (string.IsNullOrEmpty(idClaim))
      {
        // Debug: Se falhar, verifique quais claims estão chegando
        foreach (var claim in User.Claims)
        {
          Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
        }
        throw new UnauthorizedAccessException("Token inválido: Claim 'Id' não encontrada.");
      }

      if (!Guid.TryParse(idClaim, out var userId))
        throw new UnauthorizedAccessException("Token inválido: Formato do ID incorreto.");

      return userId;
    }

    // =========================
    // GET /investments
    // =========================
    [HttpGet]
    public async Task<IActionResult> BuscarTodos()
    {
      var userId = ObterUsuarioIdDoToken();
      var investments = await _service.BuscarTodosAsync(userId);
      return Ok(investments);
    }

    // =========================
    // POST /investments
    // =========================
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarInvestmentRequest request)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var userId = ObterUsuarioIdDoToken();
      var investment = await _service.CriarAsync(userId, request);

      // Não precisa passar "id" em BuscarTodos, apenas retornar o recurso criado
      return CreatedAtAction(nameof(BuscarTodos), investment);
    }

    // =========================
    // PUT /investments/{id}
    // =========================
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarInvestmentRequest request)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var userId = ObterUsuarioIdDoToken();
      await _service.AtualizarAsync(userId, id, request);

      return NoContent();
    }

    // =========================
    // DELETE /investments/{id}
    // =========================
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
      var userId = ObterUsuarioIdDoToken();
      await _service.ExcluirAsync(userId, id);

      return NoContent();
    }

    // =========================
    // Método interno para testes sem autenticação (opcional)
    // =========================
    internal async Task<IActionResult> CriarParaTeste(Guid usuarioId, CriarInvestmentRequest request)
    {
      var investment = await _service.CriarAsync(usuarioId, request);
      return CreatedAtAction(nameof(BuscarTodos), investment);
    }
  }
}
