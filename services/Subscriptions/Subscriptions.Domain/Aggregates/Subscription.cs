using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Errors;
using Subscriptions.Domain.Events;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Aggregates;

public sealed class Subscription : AggregateRoot<SubscriptionId>
{
    public GroupId GroupId { get; private init; } = null!;
    public string ServiceName { get; private set; } = null!;
    public Money TotalCost { get; private set; } = null!;
    public BillingSchedule BillingSchedule { get; private set; } = null!;
    public DateTime CreatedAt { get; private init; }
    public bool IsActive { get; private set; }

    private Subscription() { }

    public static Result<Subscription> Create(
        GroupId groupId,
        string serviceName,
        Money totalCost,
        BillingSchedule billingSchedule,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return Result.Failure<Subscription>(SubscriptionErrors.ServiceNameEmpty);

        if (serviceName.Length > 100)
            return Result.Failure<Subscription>(SubscriptionErrors.ServiceNameTooLong);

        var subscription = new Subscription
        {
            Id = SubscriptionId.New(),
            GroupId = groupId,
            ServiceName = serviceName.Trim(),
            TotalCost = totalCost,
            BillingSchedule = billingSchedule,
            CreatedAt = createdAt,
            IsActive = true
        };

        subscription.RaiseDomainEvent(new SubscriptionCreatedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: createdAt,
            SubscriptionId: subscription.Id,
            GroupId: groupId,
            ServiceName: subscription.ServiceName,
            TotalCost: totalCost));

        return Result.Success(subscription);
    }

    public Result UpdatePrice(Money newCost, DateTime occurredOn)
    {
        var oldCost = TotalCost;
        TotalCost = newCost;

        RaiseDomainEvent(new SubscriptionPriceChangedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: occurredOn,
            SubscriptionId: Id,
            OldCost: oldCost,
            NewCost: newCost));

        return Result.Success();
    }

    public Result Deactivate(DateTime occurredOn)
    {
        if (!IsActive)
            return Result.Failure(SubscriptionErrors.AlreadyInactive);

        IsActive = false;

        RaiseDomainEvent(new SubscriptionDeactivatedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: occurredOn,
            SubscriptionId: Id));

        return Result.Success();
    }

    public Result AdvanceBillingCycle(DateTime occurredOn)
    {
        BillingSchedule = BillingSchedule.CalculateNextBillingDate();

        RaiseDomainEvent(new BillingCycleAdvancedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: occurredOn,
            SubscriptionId: Id,
            NewBillingDate: BillingSchedule.NextBillingDate));

        return Result.Success();
    }
}
