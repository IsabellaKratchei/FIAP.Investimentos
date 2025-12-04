namespace FIAP.Domain.Entities
{
  public class Investment
  {
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string TipoInvestment { get; private set; } = null!;
    public Decimal ValorInvestment { get; private set; } //utilizando decimal para representar valores monetários
    public DateTimeOffset DataInvestment { get; private set; }

    public Usuario Usuario { get; private set; }

    private Investment() { }

    public Investment(Guid userId, string tipoInvestment, decimal valor, DateTimeOffset dataInvestment)
    {
      if (string.IsNullOrWhiteSpace(tipoInvestment))
        throw new ArgumentException("Tipo é obrigatório.", nameof(tipoInvestment));

      Id = Guid.NewGuid();             // Id do investimento
      IdUsuario = userId;              // Id do usuário dono do investimento
      TipoInvestment = tipoInvestment;
      ValorInvestment = valor;
      DataInvestment = dataInvestment;
    }


    public void AtualizaInvestment(string tipo, decimal valorInvestment, DateTimeOffset dataInvestment)
    {
      TipoInvestment = tipo;
      ValorInvestment = valorInvestment;
      DataInvestment = dataInvestment;
    }
  }
}
