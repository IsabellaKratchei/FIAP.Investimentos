using FIAP.Application.Interfaces;

namespace FIAP.Infraestructure.Auth
{
  public class BCryptSenhaHasher : ISenhaHasher
  {
    public string Hash(string senha)=>
      BCrypt.Net.BCrypt.HashPassword(senha);

    public bool VerificaHash(string senha, string hash)=> 
      BCrypt.Net.BCrypt.Verify(senha, hash);
  }
}
