using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FIAP.Investimentos.Tests.Helpers
{
  public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
  {
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
      // Simula sempre um usuário autenticado com um id fixo
      var claims = new[] { new Claim("id", "00000000-0000-0000-0000-000000000001") };
      var identity = new ClaimsIdentity(claims, "Test");
      var principal = new ClaimsPrincipal(identity);
      var ticket = new AuthenticationTicket(principal, "Test");

      return Task.FromResult(AuthenticateResult.Success(ticket));
    }
  }
}
