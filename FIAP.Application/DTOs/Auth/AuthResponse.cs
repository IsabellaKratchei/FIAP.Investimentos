namespace FIAP.Application.DTOs.Auth
{
  public record AuthResponse(
        string Nome,
        string Email,
        string Token,
        DateTimeOffset Expiration
    );
}
