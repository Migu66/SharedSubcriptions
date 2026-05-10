using Payments.Application.DTOs;
using Payments.Domain.Enums;
using Payments.Domain.ValueObjects;

namespace Payments.Application.Queries.GetPendingDebts;

public sealed record DebtDto(
    DebtId Id,
    PaymentRecordId PaymentRecordId,
    Guid DebtorId,
    Guid CreditorId,
    decimal Amount,
    string Currency,
    DebtStatus Status,
    PaymentStatusDto PaymentStatus,
    DateTime CreatedAt,
    DateTime? SettledAt);
