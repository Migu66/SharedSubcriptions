using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.Queries.GetPaymentHistory;

public record GetPaymentHistoryQuery(
    SubscriptionId SubscriptionId) : IRequest<Result<IReadOnlyList<PaymentRecordDto>>>;
