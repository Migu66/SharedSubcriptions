using FluentAssertions;
using Subscriptions.Domain.Enums;
using Subscriptions.Domain.Errors;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Tests.ValueObjects;

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
    public void Create_ConMayusculasOMinusculas_NormalizaAMayusculas()
    {
        // Arrange & Act
        var result = Money.Create(5m, "eur");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_ConImporteNegativo_RetornaFallo()
    {
        // Arrange & Act
        var result = Money.Create(-1m, "EUR");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(MoneyErrors.NegativeAmount.Code);
    }

    [Fact]
    public void Create_ConMonedaVacia_RetornaFallo()
    {
        // Arrange & Act
        var result = Money.Create(9.99m, "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(MoneyErrors.EmptyCurrency.Code);
    }

    [Fact]
    public void Create_ConMonedaFormatoInvalido_RetornaFallo()
    {
        // Arrange & Act
        var result = Money.Create(9.99m, "EU");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(MoneyErrors.InvalidCurrencyFormat.Code);
    }
}

public class BillingScheduleTests
{
    private static readonly DateTime FutureDate =
        new(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ConDatosValidos_RetornaExito()
    {
        // Arrange & Act
        var result = BillingSchedule.Create(BillingCycle.Monthly, FutureDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Cycle.Should().Be(BillingCycle.Monthly);
        result.Value.NextBillingDate.Should().Be(FutureDate);
    }

    [Fact]
    public void CalculateNextBillingDate_CicloMensual_AvanzaUnMes()
    {
        // Arrange
        var schedule = BillingSchedule.Create(BillingCycle.Monthly, FutureDate).Value;

        // Act
        var next = schedule.CalculateNextBillingDate();

        // Assert
        next.Cycle.Should().Be(BillingCycle.Monthly);
        next.NextBillingDate.Should().Be(FutureDate.AddMonths(1));
    }

    [Fact]
    public void CalculateNextBillingDate_CicloAnual_AvanzaUnAnho()
    {
        // Arrange
        var schedule = BillingSchedule.Create(BillingCycle.Annual, FutureDate).Value;

        // Act
        var next = schedule.CalculateNextBillingDate();

        // Assert
        next.NextBillingDate.Should().Be(FutureDate.AddYears(1));
    }
}
