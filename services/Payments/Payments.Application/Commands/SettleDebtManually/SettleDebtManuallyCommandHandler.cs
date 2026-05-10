using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Errors;
using Payments.Domain.Repositories;

namespace Payments.Application.Commands.SettleDebtManually;

internal sealed class SettleDebtManuallyCommandHandler
    : IRequestHandler<SettleDebtManuallyCommand, Result>
{
    private readonly IDebtRepository _debtRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SettleDebtManuallyCommandHandler(
        IDebtRepository debtRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _debtRepository = debtRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(
        SettleDebtManuallyCommand request,
        CancellationToken cancellationToken)
    {
        // Cargar la deuda por su ID
        var debt = await _debtRepository.GetByIdAsync(request.DebtId, cancellationToken);

        if (debt is null)
            return Result.Failure(DebtErrors.NotFound);

        // Verificar que quien confirma el cobro es el acreedor (el admin)
        if (debt.CreditorId != request.CreditorId)
            return Result.Failure(DebtErrors.NotCreditor);

        // Saldar la deuda (emite DebtSettledEvent internamente)
        var settleResult = debt.Settle(_dateTimeProvider.UtcNow);
        if (settleResult.IsFailure)
            return settleResult;

        _debtRepository.Update(debt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
