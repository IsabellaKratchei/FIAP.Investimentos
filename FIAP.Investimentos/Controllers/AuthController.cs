using FIAP.Application.DTOs.Auth;
using FIAP.Application.Interfaces;
using FIAP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Investimentos.Controllers
{
  [ApiController]
  [Route("auth")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
        => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
      try
      {
        var result = await _authService.LoginAsync(req);
        return Ok(result); 
      }
      catch (UnauthorizedAccessException)
      {
        return Unauthorized("Email e/ou senha inválidos");
      }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      try
      {
        var result = await _authService.RegistrarAsync(req);

        return Ok(result);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message);
      }
    }

  }

}
