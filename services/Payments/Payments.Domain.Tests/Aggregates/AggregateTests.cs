using FluentAssertions;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Domain.Errors;
using Payments.Domain.Events;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Tests.Aggregates;

public class PaymentRecordTests
{
    private static readonly SubscriptionId SubscriptionId = SubscriptionId.New();
    private static readonly GroupId GroupId = GroupId.New();
    private static readonly UserId AdminId = UserId.New();
    private static readonly DateTime Now = new(2026, 5, 10, 10, 0, 0, DateTimeKind.Utc);

    private static IReadOnlyList<MemberQuota> CreateQuotas(int count = 2)
    {
        var totalCost = Money.Create(9.99m, "EUR").Value;
        var quotas = new List<MemberQuota>();
        for (int i = 0; i < count; i++)
        {
            quotas.Add(MemberQuota.Calculate(UserId.New(), totalCost, count).Value);
        }
        return quotas.AsReadOnly();
    }

    [Fact]
    public void Create_ConDatosValidos_RetornaExito()
    {
        // Arrange
        var totalAmount = Money.Create(9.99m, "EUR").Value;
        var quotas = CreateQuotas(2);

        // Act
        var result = PaymentRecord.Create(SubscriptionId, GroupId, AdminId, totalAmount, quotas, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SubscriptionId.Should().Be(SubscriptionId);
        result.Value.AdminId.Should().Be(AdminId);
        result.Value.MemberQuotas.Should().HaveCount(2);
    }

    [Fact]
    public void Create_ConDatosValidos_EmitePaymentRecordCreatedEvent()
    {
        // Arrange
        var totalAmount = Money.Create(9.99m, "EUR").Value;
        var quotas = CreateQuotas(2);

        // Act
        var result = PaymentRecord.Create(SubscriptionId, GroupId, AdminId, totalAmount, quotas, Now);

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        result.Value.DomainEvents.First().Should().BeOfType<PaymentRecordCreatedEvent>();
    }

    [Fact]
    public void Create_SinCuotas_RetornaFallo()
    {
        // Arrange
        var totalAmount = Money.Create(9.99m, "EUR").Value;
        var quotasVacias = new List<MemberQuota>().AsReadOnly();

        // Act
        var result = PaymentRecord.Create(SubscriptionId, GroupId, AdminId, totalAmount, quotasVacias, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentRecordErrors.EmptyQuotas);
    }
}

public class DebtTests
{
    private static readonly PaymentRecordId PaymentRecordId = PaymentRecordId.New();
    private static readonly UserId DebtorId = UserId.New();
    private static readonly UserId CreditorId = UserId.New();
    private static readonly DateTime Now = new(2026, 5, 10, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ConDatosValidos_RetornaExito()
    {
        // Arrange
        var amount = Money.Create(4.99m, "EUR").Value;

        // Act
        var result = Debt.Create(PaymentRecordId, DebtorId, CreditorId, amount, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(DebtStatus.Pending);
        result.Value.SettledAt.Should().BeNull();
    }

    [Fact]
    public void Settle_ConDeudaPendiente_RetornaExitoYEmiteEvento()
    {
        // Arrange
        var amount = Money.Create(4.99m, "EUR").Value;
        var debt = Debt.Create(PaymentRecordId, DebtorId, CreditorId, amount, Now).Value;

        // Act
        var result = debt.Settle(Now.AddDays(1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        debt.Status.Should().Be(DebtStatus.Settled);
        debt.SettledAt.Should().Be(Now.AddDays(1));
        debt.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<DebtSettledEvent>();
    }

    [Fact]
    public void Settle_ConDeudaYaSaldada_RetornaFallo()
    {
        // Arrange
        var amount = Money.Create(4.99m, "EUR").Value;
        var debt = Debt.Create(PaymentRecordId, DebtorId, CreditorId, amount, Now).Value;
        debt.Settle(Now.AddDays(1));

        // Act
        var result = debt.Settle(Now.AddDays(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DebtErrors.AlreadySettled);
    }

    [Fact]
    public void Cancel_ConDeudaPendiente_RetornaExito()
    {
        // Arrange
        var amount = Money.Create(4.99m, "EUR").Value;
        var debt = Debt.Create(PaymentRecordId, DebtorId, CreditorId, amount, Now).Value;

        // Act
        var result = debt.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        debt.Status.Should().Be(DebtStatus.Cancelled);
    }
}
