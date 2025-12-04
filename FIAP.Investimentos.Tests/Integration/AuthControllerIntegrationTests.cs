using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FIAP.Application.DTOs.Auth;
using FIAP.Application.DTOs.Investments;
using FIAP.Application.Interfaces;
using FIAP.Infraestructure.Context;
using FIAP.Investimentos.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FIAP.Investimentos.Tests.Integration
{
  public class TestWebApplicationFactory : WebApplicationFactory<Program>
  {
    private readonly SqliteConnection _connection;

    public TestWebApplicationFactory()
    {
      _connection = new SqliteConnection("DataSource=:memory:");
      _connection.Open();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
      builder.ConfigureServices(services =>
      {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
        if (descriptor != null) services.Remove(descriptor);

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
      });
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing) _connection?.Dispose();
    }
  }

  [TestFixture]
  public class AuthControllerIntegrationTests
  {
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
      _factory = new TestWebApplicationFactory();
      _client = _factory.CreateClient();

      // Inicializando o controller para testes diretos
      var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
      using var scope = scopeFactory.CreateScope();
      var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

      _controller = new AuthController(authService);
    }

    [TearDown]
    public void TearDown()
    {
      _client.Dispose();
      _factory.Dispose();
    }

    private StringContent ToJsonContent(object obj)
        => new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

    //[Test]
    //public async Task Register_Login_And_AccessProtectedEndpoint_EndToEnd()
    //{
    //  // =========================
    //  // ARRANGE — Registro
    //  // =========================
    //  var registerRequest = new RegisterRequest("Test User", "testuser@example.com", "Senha123!");
    //  var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerRequest);

    //  Assert.IsTrue(registerResponse.StatusCode == HttpStatusCode.Created || registerResponse.StatusCode == HttpStatusCode.OK);

    //  var registerValue = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
    //  Assert.IsNotNull(registerValue);
    //  Assert.AreEqual(registerRequest.Email, registerValue!.Email);
    //  Assert.AreEqual(registerRequest.Nome, registerValue.Nome);

    //  // =========================
    //  // ACT — Login
    //  // =========================
    //  var loginRequest = new LoginRequest(registerRequest.Email, registerRequest.Senha);
    //  var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginRequest);

    //  Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

    //  var loginValue = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
    //  Assert.IsNotNull(loginValue);
    //  Assert.AreEqual(registerRequest.Email, loginValue!.Email);
    //  Assert.IsFalse(string.IsNullOrEmpty(loginValue.Token));

    //  // =========================
    //  // ACT — Chamada de endpoint protegido (Investments)
    //  // =========================
    //  _client.DefaultRequestHeaders.Authorization =
    //      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginValue.Token);

    //  var protectedResponse = await _client.GetAsync("/investments"); // ajuste a rota de acordo com seu InvestmentsController
    //  Assert.AreEqual(HttpStatusCode.OK, protectedResponse.StatusCode);

    //  var protectedValue = await protectedResponse.Content.ReadFromJsonAsync<IEnumerable<InvestmentResponse>>();
    //  Assert.IsNotNull(protectedValue);
    //}

    [Test]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
      var request = new RegisterRequest("Teste", "teste@teste.com", "123456");

      // Primeiro registro
      var firstResponse = await _client.PostAsJsonAsync("/auth/register", request);
      Assert.IsTrue(firstResponse.StatusCode == HttpStatusCode.Created || firstResponse.StatusCode == HttpStatusCode.OK);

      // Segundo registro (duplicado)
      var duplicateResponse = await _client.PostAsJsonAsync("/auth/register", request);
      Assert.AreEqual(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);

      var message = await duplicateResponse.Content.ReadAsStringAsync();
      Assert.IsTrue(message.Contains("Email já está em uso"));
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
      var loginReq = new { Email = "notexists@local", Senha = "wrong" };
      var resp = await _client.PostAsync("/auth/login", ToJsonContent(loginReq));
      Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
  }
}