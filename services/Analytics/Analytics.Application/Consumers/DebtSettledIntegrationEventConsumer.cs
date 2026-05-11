using Analytics.Application.IntegrationEvents;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using MassTransit;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Consumers;

/// <summary>
/// Consume DebtSettledIntegrationEvent y actualiza DebtHistoryReadModel
/// marcando una deuda como saldada para el deudor correspondiente.
/// </summary>
internal sealed class DebtSettledIntegrationEventConsumer
    : IConsumer<DebtSettledIntegrationEvent>
{
    private readonly IDebtHistoryRepository _debtHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DebtSettledIntegrationEventConsumer(
        IDebtHistoryRepository debtHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _debtHistoryRepository = debtHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<DebtSettledIntegrationEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        var userIdResult = UserId.From(evt.DebtorId);
        if (userIdResult.IsFailure)
            return;

        var userId = userIdResult.Value!;

        var debtHistory = await _debtHistoryRepository.GetByUserIdAsync(userId, cancellationToken);
        if (debtHistory is null)
            return;

        debtHistory.SettleDebt(evt.Amount);
        await _debtHistoryRepository.UpdateAsync(debtHistory, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
