namespace FIAP.Application.DTOs.Auth
{
  public record LoginRequest(
    string Email,
     string Senha
  );
}
