using System;
using FIAP.Application.DTOs.Investments;
using NUnit.Framework;

namespace FIAP.Tests.DTOs
{
  public class CriarInvestmentRequestTests
  {
    [Test]
    public void Deve_Criar_DTO_Com_Valores_Corretos()
    {
      // Arrange
      var tipo = "RendaFixa";
      var valor = 1500.75m;
      var data = DateTimeOffset.UtcNow;

      // Act
      var dto = new CriarInvestmentRequest
      {
        TipoInvestment = tipo,
        ValorInvestment = valor,
        DataInvestment = data
      };

      // Assert
      Assert.AreEqual(tipo, dto.TipoInvestment);
      Assert.AreEqual(valor, dto.ValorInvestment);
      Assert.AreEqual(data, dto.DataInvestment);
    }

    [Test]
    public void DTO_Deve_Permitir_Alterar_Propriedades()
    {
      // Arrange
      var dto = new CriarInvestmentRequest
      {
        TipoInvestment = "RendaVariavel",
        ValorInvestment = 500,
        DataInvestment = DateTimeOffset.Now
      };

      // Act
      dto.TipoInvestment = "Cripto";
      dto.ValorInvestment = 999.99m;

      // Assert
      Assert.AreEqual("Cripto", dto.TipoInvestment);
      Assert.AreEqual(999.99m, dto.ValorInvestment);
    }
  }
}
