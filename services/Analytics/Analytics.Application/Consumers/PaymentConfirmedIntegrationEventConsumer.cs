using Analytics.Application.IntegrationEvents;
using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using MassTransit;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Consumers;

/// <summary>
/// Consume PaymentConfirmedIntegrationEvent y actualiza GroupSavingsReadModel
/// y ServiceSpendingReadModel con los datos del pago confirmado.
/// </summary>
internal sealed class PaymentConfirmedIntegrationEventConsumer
    : IConsumer<PaymentConfirmedIntegrationEvent>
{
    private readonly IGroupSavingsRepository _groupSavingsRepository;
    private readonly IServiceSpendingRepository _serviceSpendingRepository;
    private readonly ISubscriptionContextRepository _subscriptionContextRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentConfirmedIntegrationEventConsumer(
        IGroupSavingsRepository groupSavingsRepository,
        IServiceSpendingRepository serviceSpendingRepository,
        ISubscriptionContextRepository subscriptionContextRepository,
        IUnitOfWork unitOfWork)
    {
        _groupSavingsRepository = groupSavingsRepository;
        _serviceSpendingRepository = serviceSpendingRepository;
        _subscriptionContextRepository = subscriptionContextRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<PaymentConfirmedIntegrationEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        var groupIdResult = GroupId.From(evt.GroupId);
        if (groupIdResult.IsFailure)
            return;

        var groupId = groupIdResult.Value!;
        var year = DateTime.UtcNow.Year;

        // Actualizar ahorro anual del grupo
        var savings = await _groupSavingsRepository
            .GetByGroupIdAndYearAsync(groupId, year, cancellationToken)
            ?? GroupSavingsReadModel.Create(groupId, year);

        var memberCount = evt.Quotas.Count > 0 ? evt.Quotas.Count : 1;
        var estimatedIndividualCost = evt.TotalAmount; // lo que pagaría una persona sola
        var totalSharedCost = evt.TotalAmount / memberCount;
        var estimatedSavingPerMember = estimatedIndividualCost - totalSharedCost;

        savings.AddPayment(totalSharedCost, estimatedSavingPerMember);

        if (savings.TotalSpent == totalSharedCost)
            await _groupSavingsRepository.AddAsync(savings, cancellationToken);
        else
            await _groupSavingsRepository.UpdateAsync(savings, cancellationToken);

        // Actualizar gasto por servicio
        var subscriptionContext = await _subscriptionContextRepository
            .GetBySubscriptionIdAsync(evt.SubscriptionId, cancellationToken);

        var serviceName = subscriptionContext?.ServiceName ?? "Desconocido";

        var serviceSpending = await _serviceSpendingRepository
            .GetByGroupIdAndServiceNameAsync(groupId, serviceName, cancellationToken)
            ?? ServiceSpendingReadModel.Create(groupId, serviceName);

        serviceSpending.RecordPayment(evt.TotalAmount);

        if (serviceSpending.PaymentCount == 1)
            await _serviceSpendingRepository.AddAsync(serviceSpending, cancellationToken);
        else
            await _serviceSpendingRepository.UpdateAsync(serviceSpending, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
