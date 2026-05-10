using FluentAssertions;
using Payments.Domain.Errors;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Tests.ValueObjects;

public class MemberQuotaTests
{
    private static readonly UserId MemberId = UserId.New();

    [Fact]
    public void Calculate_ConDatosValidos_RetornaCuotaCorrecta()
    {
        // Arrange
        var totalCost = Money.Create(9.99m, "EUR").Value;

        // Act
        var result = MemberQuota.Calculate(MemberId, totalCost, memberCount: 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(3.33m);
        result.Value.Currency.Should().Be("EUR");
        result.Value.IsProrrated.Should().BeFalse();
    }

    [Fact]
    public void Calculate_ConCeroMiembros_RetornaFallo()
    {
        // Arrange
        var totalCost = Money.Create(9.99m, "EUR").Value;

        // Act
        var result = MemberQuota.Calculate(MemberId, totalCost, memberCount: 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MemberQuotaErrors.InvalidMemberCount);
    }

    [Fact]
    public void CalculateProrrated_ConDatosValidos_RetornaCuotaProrateada()
    {
        // Arrange
        var totalCost = Money.Create(9.99m, "EUR").Value;

        // Act — 15 días restantes de 30 (mitad del mes)
        var result = MemberQuota.CalculateProrrated(MemberId, totalCost, memberCount: 2, remainingDays: 15, totalDays: 30);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(2.50m); // (9.99/2) * (15/30) = 2.50 aprox
        result.Value.IsProrrated.Should().BeTrue();
    }

    [Fact]
    public void CalculateProrrated_ConDiasNegativos_RetornaFallo()
    {
        // Arrange
        var totalCost = Money.Create(9.99m, "EUR").Value;

        // Act
        var result = MemberQuota.CalculateProrrated(MemberId, totalCost, memberCount: 2, remainingDays: -1, totalDays: 30);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MemberQuotaErrors.InvalidDays);
    }
}

public class MoneyTests
{
    [Fact]
    public void Create_ConDatosValidos_RetornaExito()
    {
        // Arrange & Act
        var result = Money.Create(9.99m, "EUR");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(9.99m);
        result.Value.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_ConImporteNegativo_RetornaFallo()
    {
        // Arrange & Act
        var result = Money.Create(-1m, "EUR");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MoneyErrors.NegativeAmount);
    }

    [Fact]
    public void Create_ConMonedaInvalida_RetornaFallo()
    {
        // Arrange & Act
        var result = Money.Create(9.99m, "EU"); // Solo 2 letras, inválido

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MoneyErrors.InvalidCurrencyFormat);
    }
}
