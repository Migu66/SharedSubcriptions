using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Errors;
using Payments.Domain.Repositories;

namespace Payments.Application.Queries.GetPaymentHistory;

internal sealed class GetPaymentHistoryQueryHandler
    : IRequestHandler<GetPaymentHistoryQuery, Result<IReadOnlyList<PaymentRecordDto>>>
{
    private readonly IPaymentRecordRepository _paymentRecordRepository;

    public GetPaymentHistoryQueryHandler(IPaymentRecordRepository paymentRecordRepository)
    {
        _paymentRecordRepository = paymentRecordRepository;
    }

    public async Task<Result<IReadOnlyList<PaymentRecordDto>>> Handle(
        GetPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var records = await _paymentRecordRepository.GetBySubscriptionIdAsync(
            request.SubscriptionId, cancellationToken);

        var dtos = records
            .Select(r => new PaymentRecordDto(
                Id: r.Id,
                SubscriptionId: r.SubscriptionId,
                GroupId: r.GroupId,
                AdminId: r.AdminId,
                TotalAmount: r.TotalAmount.Amount,
                Currency: r.TotalAmount.Currency,
                PaidAt: r.PaidAt,
                MemberQuotas: r.MemberQuotas
                    .Select(q => new MemberQuotaDto(
                        MemberId: q.MemberId.Value,
                        Amount: q.Amount,
                        Currency: q.Currency,
                        IsProrrated: q.IsProrrated))
                    .ToList()
                    .AsReadOnly()))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<PaymentRecordDto>>(dtos);
    }
}
