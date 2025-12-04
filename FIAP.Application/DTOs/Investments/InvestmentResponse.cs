namespace FIAP.Application.DTOs.Investments
{
  public record InvestmentResponse
  {
    public Guid Id { get; set; }
    public string TipoInvestment { get; set; }
    public decimal ValorInvestment { get; set; }
    public DateTimeOffset DataInvestment { get; set; }
  }
}
