using MassTransit;
using MediatR;
using Payments.Application.IntegrationEvents;
using Payments.Domain.Events;

namespace Payments.Infrastructure.EventHandlers;

internal sealed class PaymentRecordCreatedEventHandler
    : INotificationHandler<PaymentRecordCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentRecordCreatedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        PaymentRecordCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        var quotaDtos = notification.Quotas
            .Select(q => new MemberQuotaIntegrationDto(
                MemberId: q.MemberId.Value,
                Amount: q.Amount,
                Currency: q.Currency,
                IsProrrated: q.IsProrrated))
            .ToList()
            .AsReadOnly();

        var integrationEvent = new PaymentConfirmedIntegrationEvent(
            paymentRecordId: notification.PaymentRecordId,
            subscriptionId: notification.SubscriptionId,
            groupId: notification.GroupId,
            adminId: notification.AdminId.Value,
            totalAmount: notification.Quotas.Sum(q => q.Amount),
            currency: notification.Quotas.FirstOrDefault()?.Currency ?? string.Empty,
            quotas: quotaDtos);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        // Publicar un DebtCreatedIntegrationEvent por cada cuota (excluye al admin/acreedor)
        foreach (var quota in notification.Quotas)
        {
            if (quota.MemberId == notification.AdminId)
                continue;

            var debtEvent = new DebtCreatedIntegrationEvent(
                debtId: Payments.Domain.ValueObjects.DebtId.New(),
                subscriptionId: notification.SubscriptionId,
                debtorId: quota.MemberId.Value,
                creditorId: notification.AdminId.Value,
                amount: quota.Amount,
                currency: quota.Currency);

            await _publishEndpoint.Publish(debtEvent, cancellationToken);
        }
    }
}
