using Payments.Domain.ValueObjects;

namespace Payments.Application.Queries.GetPaymentHistory;

public sealed record PaymentRecordDto(
    PaymentRecordId Id,
    SubscriptionId SubscriptionId,
    GroupId GroupId,
    UserId AdminId,
    decimal TotalAmount,
    string Currency,
    DateTime PaidAt,
    IReadOnlyList<MemberQuotaDto> MemberQuotas);

public sealed record MemberQuotaDto(
    Guid MemberId,
    decimal Amount,
    string Currency,
    bool IsProrrated);
