using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Application.Interfaces
{
  public interface ISenhaHasher
  {
    string Hash(string senha);
    bool VerificaHash(string senha, string hash);
  }
}
