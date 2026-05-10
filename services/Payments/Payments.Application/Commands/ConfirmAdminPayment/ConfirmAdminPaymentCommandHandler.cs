using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Aggregates;
using Payments.Domain.Errors;
using Payments.Domain.Repositories;
using Payments.Domain.ValueObjects;

namespace Payments.Application.Commands.ConfirmAdminPayment;

internal sealed class ConfirmAdminPaymentCommandHandler
    : IRequestHandler<ConfirmAdminPaymentCommand, Result<PaymentRecordId>>
{
    private readonly IPaymentRecordRepository _paymentRecordRepository;
    private readonly IDebtRepository _debtRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ConfirmAdminPaymentCommandHandler(
        IPaymentRecordRepository paymentRecordRepository,
        IDebtRepository debtRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _paymentRecordRepository = paymentRecordRepository;
        _debtRepository = debtRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<PaymentRecordId>> Handle(
        ConfirmAdminPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // Construir el objeto Money con el importe total pagado al proveedor
        var moneyResult = Money.Create(request.TotalAmount, request.Currency);
        if (moneyResult.IsFailure)
            return Result.Failure<PaymentRecordId>(moneyResult.Error);

        var totalAmount = moneyResult.Value;

        // Calcular la cuota proporcional de cada miembro
        var memberQuotas = new List<MemberQuota>();
        int memberCount = request.MemberIds.Count;

        foreach (var memberGuid in request.MemberIds)
        {
            var memberIdResult = UserId.From(memberGuid);
            if (memberIdResult.IsFailure)
                return Result.Failure<PaymentRecordId>(memberIdResult.Error);

            var quotaResult = MemberQuota.Calculate(memberIdResult.Value, totalAmount, memberCount);
            if (quotaResult.IsFailure)
                return Result.Failure<PaymentRecordId>(quotaResult.Error);

            memberQuotas.Add(quotaResult.Value);
        }

        // Crear el registro de pago del administrador
        var paymentRecordResult = PaymentRecord.Create(
            request.SubscriptionId,
            request.GroupId,
            request.AdminId,
            totalAmount,
            memberQuotas.AsReadOnly(),
            request.PaidAt);

        if (paymentRecordResult.IsFailure)
            return Result.Failure<PaymentRecordId>(paymentRecordResult.Error);

        var paymentRecord = paymentRecordResult.Value;
        await _paymentRecordRepository.AddAsync(paymentRecord, cancellationToken);

        // Generar una deuda individual para cada miembro (excluyendo al propio admin)
        foreach (var quota in memberQuotas)
        {
            if (quota.MemberId == request.AdminId)
                continue;

            var debtAmountResult = Money.Create(quota.Amount, quota.Currency);
            if (debtAmountResult.IsFailure)
                return Result.Failure<PaymentRecordId>(debtAmountResult.Error);

            var debtResult = Debt.Create(
                paymentRecord.Id,
                debtorId: quota.MemberId,
                creditorId: request.AdminId,
                amount: debtAmountResult.Value,
                createdAt: request.PaidAt);

            if (debtResult.IsFailure)
                return Result.Failure<PaymentRecordId>(debtResult.Error);

            await _debtRepository.AddAsync(debtResult.Value, cancellationToken);
        }

        // Persistir todo en una sola transacción
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(paymentRecord.Id);
    }
}
