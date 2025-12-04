using FIAP.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Application.Interfaces
{
  public interface IAuthenticationService
  {
    Task<AuthResponse> RegistrarAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
  }
}
