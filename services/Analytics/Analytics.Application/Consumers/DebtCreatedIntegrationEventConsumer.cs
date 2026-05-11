using Analytics.Application.IntegrationEvents;
using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using MassTransit;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Consumers;

/// <summary>
/// Consume DebtCreatedIntegrationEvent y actualiza DebtHistoryReadModel
/// incrementando el contador de deudas pendientes del deudor.
/// </summary>
internal sealed class DebtCreatedIntegrationEventConsumer
    : IConsumer<DebtCreatedIntegrationEvent>
{
    private readonly IDebtHistoryRepository _debtHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DebtCreatedIntegrationEventConsumer(
        IDebtHistoryRepository debtHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _debtHistoryRepository = debtHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<DebtCreatedIntegrationEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        var userIdResult = UserId.From(evt.DebtorId);
        if (userIdResult.IsFailure)
            return;

        var userId = userIdResult.Value!;

        var debtHistory = await _debtHistoryRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? DebtHistoryReadModel.Create(userId);

        debtHistory.AddDebt(evt.Amount);

        if (debtHistory.TotalDebt == evt.Amount)
            await _debtHistoryRepository.AddAsync(debtHistory, cancellationToken);
        else
            await _debtHistoryRepository.UpdateAsync(debtHistory, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
