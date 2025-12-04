using FIAP.Application.DTOs.Auth;
using FIAP.Application.Interfaces;
using FIAP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Application.Services
{
  public class AuthenticationService : IAuthenticationService
  {
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(IUsuarioRepository usuarioRepository,
        ISenhaHasher senhaHasher,
        IJwtTokenService jwtTokenService)
    {
      _usuarioRepository = usuarioRepository;
      _senhaHasher = senhaHasher;
      _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
      var usuario = await _usuarioRepository.BuscarPorEmailAsync(request.Email);
      if (usuario == null)
        throw new UnauthorizedAccessException("Credenciais inválidas.");

      if (!_senhaHasher.VerificaHash(request.Senha, usuario.SenhaHash))
        throw new UnauthorizedAccessException("Credenciais inválidas.");

      var token = _jwtTokenService.GenerateToken(usuario);
      var expiration = DateTimeOffset.UtcNow.AddHours(1);

      return new AuthResponse(
          usuario.Nome,
          usuario.Email,
          token,
          expiration
      );
    }

    public async Task<AuthResponse> RegistrarAsync(RegisterRequest request)
    {
      var usuarioExistente = await _usuarioRepository.BuscarPorEmailAsync(request.Email);
      if (usuarioExistente != null)
        throw new InvalidOperationException("Email já está em uso.");

      var hash = _senhaHasher.Hash(request.Senha);

      var usuario = new Usuario(
          request.Nome,
          hash,
          request.Email
      );

      await _usuarioRepository.AdicionarAsync(usuario);
      await _usuarioRepository.SalvarMudancasAsync();

      var token = _jwtTokenService.GenerateToken(usuario);
      var expiration = DateTimeOffset.UtcNow.AddHours(1);

      return new AuthResponse(
          usuario.Nome,
          usuario.Email,
          token,
          expiration
      );
    }

  }
}
