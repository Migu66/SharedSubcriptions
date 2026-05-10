using FluentAssertions;
using Subscriptions.Domain.Aggregates;
using Subscriptions.Domain.Enums;
using Subscriptions.Domain.Errors;
using Subscriptions.Domain.Events;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Tests.Aggregates;

public class SubscriptionTests
{
    private static readonly GroupId GroupId = GroupId.New();
    private static readonly UserId AdminId = UserId.New();
    private static readonly DateTime Now = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    private static Subscription CreateValidSubscription()
    {
        var money = Money.Create(9.99m, "EUR").Value;
        var schedule = BillingSchedule.Create(BillingCycle.Monthly, Now.AddMonths(1)).Value;
        return Subscription.Create(GroupId, "Netflix", money, schedule, Now).Value;
    }

    [Fact]
    public void Create_ConDatosValidos_RetornaExito()
    {
        // Arrange
        var money = Money.Create(9.99m, "EUR").Value;
        var schedule = BillingSchedule.Create(BillingCycle.Monthly, Now.AddMonths(1)).Value;

        // Act
        var result = Subscription.Create(GroupId, "Netflix", money, schedule, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ServiceName.Should().Be("Netflix");
        result.Value.IsActive.Should().BeTrue();
        result.Value.GroupId.Should().Be(GroupId);
    }

    [Fact]
    public void Create_ConDatosValidos_EmiteSubscriptionCreatedEvent()
    {
        // Arrange
        var money = Money.Create(9.99m, "EUR").Value;
        var schedule = BillingSchedule.Create(BillingCycle.Monthly, Now.AddMonths(1)).Value;

        // Act
        var result = Subscription.Create(GroupId, "Netflix", money, schedule, Now);

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        result.Value.DomainEvents.First().Should().BeOfType<SubscriptionCreatedEvent>();
    }

    [Fact]
    public void Create_ConNombreVacio_RetornaFallo()
    {
        // Arrange
        var money = Money.Create(9.99m, "EUR").Value;
        var schedule = BillingSchedule.Create(BillingCycle.Monthly, Now.AddMonths(1)).Value;

        // Act
        var result = Subscription.Create(GroupId, "", money, schedule, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SubscriptionErrors.ServiceNameEmpty.Code);
    }

    [Fact]
    public void UpdatePrice_ConPrecioValido_ActualizaElCoste()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var newMoney = Money.Create(12.99m, "EUR").Value;

        // Act
        var result = subscription.UpdatePrice(newMoney, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.TotalCost.Amount.Should().Be(12.99m);
    }

    [Fact]
    public void UpdatePrice_ConPrecioValido_EmiteSubscriptionPriceChangedEvent()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ClearDomainEvents();
        var newMoney = Money.Create(12.99m, "EUR").Value;

        // Act
        subscription.UpdatePrice(newMoney, Now);

        // Assert
        subscription.DomainEvents.Should().ContainSingle();
        subscription.DomainEvents.First().Should().BeOfType<SubscriptionPriceChangedEvent>();

        var evt = (SubscriptionPriceChangedEvent)subscription.DomainEvents.First();
        evt.OldCost.Amount.Should().Be(9.99m);
        evt.NewCost.Amount.Should().Be(12.99m);
    }

    [Fact]
    public void Deactivate_SuscripcionActiva_DesactivaCorrectamente()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ClearDomainEvents();

        // Act
        var result = subscription.Deactivate(Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_SuscripcionYaInactiva_RetornaFallo()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Deactivate(Now);
        subscription.ClearDomainEvents();

        // Act
        var result = subscription.Deactivate(Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SubscriptionErrors.AlreadyInactive.Code);
    }

    [Fact]
    public void AdvanceBillingCycle_CicloMensual_AvanzaUnMes()
    {
        // Arrange
        var billingDate = new DateTime(2026, 6, 8, 0, 0, 0, DateTimeKind.Utc);
        var money = Money.Create(9.99m, "EUR").Value;
        var schedule = BillingSchedule.Create(BillingCycle.Monthly, billingDate).Value;
        var subscription = Subscription.Create(GroupId, "Netflix", money, schedule, Now).Value;
        subscription.ClearDomainEvents();

        // Act
        subscription.AdvanceBillingCycle(Now);

        // Assert
        subscription.BillingSchedule.NextBillingDate.Should().Be(billingDate.AddMonths(1));
    }
}
