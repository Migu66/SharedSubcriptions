using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.Commands.ConfirmAdminPayment;

public record ConfirmAdminPaymentCommand(
    SubscriptionId SubscriptionId,
    GroupId GroupId,
    UserId AdminId,
    IReadOnlyList<Guid> MemberIds,
    decimal TotalAmount,
    string Currency,
    DateTime PaidAt) : IRequest<Result<PaymentRecordId>>;
