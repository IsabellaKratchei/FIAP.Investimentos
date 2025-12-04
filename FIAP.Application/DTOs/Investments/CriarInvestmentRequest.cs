namespace FIAP.Application.DTOs.Investments
{
  public record CriarInvestmentRequest
  {
    public string TipoInvestment { get; set; }
    public decimal ValorInvestment { get; set; }
    public DateTimeOffset DataInvestment { get; set; }
  }
}
