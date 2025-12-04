using FIAP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Application.Interfaces
{
  public interface IJwtTokenService
  {
    string GenerateToken(Usuario usuario);
  }
}
