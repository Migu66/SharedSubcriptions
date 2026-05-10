using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Application.DTOs;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;

namespace Payments.Application.Queries.GetPendingDebts;

internal sealed class GetPendingDebtsQueryHandler
    : IRequestHandler<GetPendingDebtsQuery, Result<IReadOnlyList<DebtDto>>>
{
    private readonly IDebtRepository _debtRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetPendingDebtsQueryHandler(
        IDebtRepository debtRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _debtRepository = debtRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<DebtDto>>> Handle(
        GetPendingDebtsQuery request,
        CancellationToken cancellationToken)
    {
        var debts = await _debtRepository.GetPendingByDebtorIdAsync(
            request.UserId, cancellationToken);

        var now = _dateTimeProvider.UtcNow;

        var dtos = debts
            .Select(d => new DebtDto(
                Id: d.Id,
                PaymentRecordId: d.PaymentRecordId,
                DebtorId: d.DebtorId.Value,
                CreditorId: d.CreditorId.Value,
                Amount: d.Amount.Amount,
                Currency: d.Amount.Currency,
                Status: d.Status,
                PaymentStatus: CalculatePaymentStatus(d.Status, d.CreatedAt, now),
                CreatedAt: d.CreatedAt,
                SettledAt: d.SettledAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<DebtDto>>(dtos);
    }

    private static PaymentStatusDto CalculatePaymentStatus(
        DebtStatus status,
        DateTime createdAt,
        DateTime now)
    {
        if (status == DebtStatus.Settled)
            return PaymentStatusDto.Green;

        // Si la deuda tiene menos de 1 día de antigüedad → amarillo (pendiente pero reciente)
        if ((now - createdAt).TotalDays <= 1)
            return PaymentStatusDto.Yellow;

        // Pasado 1 día sin saldar → rojo (moroso)
        return PaymentStatusDto.Red;
    }
}
