using FIAP.Application.Interfaces;
using FIAP.Application.Services;
using FIAP.Infraestructure.Auth;
using FIAP.Infraestructure.Context;
using FIAP.Infraestructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FIAP.Investimentos
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // =========================
      // Configuração do DbContext
      // =========================
      builder.Services.AddDbContext<AppDbContext>(opts =>
          opts.UseSqlServer(
              builder.Configuration.GetConnectionString("DefaultConnection"),
              b => b.MigrationsAssembly("FIAP.Infraestructure")
          )
      );
      JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

      // =========================
      // DI: Repositórios e Services
      // =========================
      builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
      builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();
      builder.Services.AddScoped<ISenhaHasher, BCryptSenhaHasher>();
      builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
      builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

      // =========================
      // JWT Auth
      // =========================
      //var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

      builder.Services.AddAuthentication(options => // Boa prática: definir os defaults aqui
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        // Mantenha seus parâmetros de validação aqui...
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = builder.Configuration["Jwt:Issuer"],
          ValidAudience = builder.Configuration["Jwt:Audience"],

          // CORREÇÃO: Padronize a leitura da chave
          IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])
        ),

          ClockSkew = TimeSpan.Zero
        };

        // === ADICIONE ISTO PARA DEBUGAR ===
        options.Events = new JwtBearerEvents
        {
          //OnMessageReceived = ctx =>
          //{
          //  var header = ctx.Request.Headers["Authorization"].ToString();

          //  // Logar apenas para referência, o que você já fez:
          //  // Console.WriteLine($"[DEBUG] Auth Header: '{header ?? "NULO"}'"); 

          //  // === FORÇAR A EXTRAÇÃO DO TOKEN COM LÓGICA MANUAL ===
          //  if (!string.IsNullOrEmpty(header) &&
          //      header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
          //  {
          //    // Remove 'Bearer ' e qualquer espaço adjacente
          //    ctx.Token = header.Substring("Bearer ".Length).Trim();
          //  }
          //  // ==================================================

          //  return Task.CompletedTask;
          //},
          OnMessageReceived = ctx =>
          {
            // Tenta obter o token da Query String (para teste)
            var accessToken = ctx.Request.Query["access_token"];

            // Se a query string tiver o token, use-o
            if (!string.IsNullOrEmpty(accessToken))
            {
              ctx.Token = accessToken;
            }

            // Caso contrário, use o Header Authorization (sua lógica original)
            if (string.IsNullOrEmpty(ctx.Token))
            {
              var header = ctx.Request.Headers["Authorization"].ToString();
              if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
              {
                ctx.Token = header.Substring("Bearer ".Length).Trim();
              }
            }
            return Task.CompletedTask;
          },
          OnAuthenticationFailed = context =>
          {
            Console.WriteLine("=================================");
            Console.WriteLine($"ERRO JWT: {context.Exception.Message}");
            Console.WriteLine("=================================");
            return Task.CompletedTask;
          },
          OnTokenValidated = context =>
          {
            Console.WriteLine("=== TOKEN VALIDADO COM SUCESSO ===");
            return Task.CompletedTask;
          }


        };
      });

      // =========================
      // Controllers & Swagger
      // =========================
      builder.Services.AddControllers();
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FIAP Investments API", Version = "v1" });

        // Configuração mais amigável
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
          Description = "Insira o token JWT aqui (não precisa digitar 'Bearer ')",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.Http, // Mude de ApiKey para Http
          Scheme = "Bearer",
          BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
          });

      var app = builder.Build();

      // =========================
      // Middleware
      // =========================
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();
      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllers();
      app.Run();
    }
  }
}