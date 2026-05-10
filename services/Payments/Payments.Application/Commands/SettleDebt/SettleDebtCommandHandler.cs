using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Errors;
using Payments.Domain.Repositories;

namespace Payments.Application.Commands.SettleDebt;

internal sealed class SettleDebtCommandHandler
    : IRequestHandler<SettleDebtCommand, Result>
{
    private readonly IDebtRepository _debtRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SettleDebtCommandHandler(
        IDebtRepository debtRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _debtRepository = debtRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(
        SettleDebtCommand request,
        CancellationToken cancellationToken)
    {
        // Cargar la deuda por su ID
        var debt = await _debtRepository.GetByIdAsync(request.DebtId, cancellationToken);

        if (debt is null)
            return Result.Failure(DebtErrors.NotFound);

        // Verificar que quien salda la deuda es el propio deudor
        if (debt.DebtorId != request.DebtorId)
            return Result.Failure(DebtErrors.NotDebtor);

        // Saldar la deuda (emite DebtSettledEvent internamente)
        var settleResult = debt.Settle(_dateTimeProvider.UtcNow);
        if (settleResult.IsFailure)
            return settleResult;

        _debtRepository.Update(debt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
