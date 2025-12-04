namespace FIAP.Domain.Entities
{
  public class Usuario
  {
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string SenhaHash { get; set; }
    public string Email { get; set; }

    public ICollection<Investment> Investments { get; set; } = new List<Investment>();

    private Usuario() { }

    public Usuario(string nome,string senhaHash, string email)
    {
      Nome = nome;
      SenhaHash = senhaHash;
      Email = email;
    }

    public void AtualizarSenha(string passHash) => SenhaHash = passHash;

  }
}
