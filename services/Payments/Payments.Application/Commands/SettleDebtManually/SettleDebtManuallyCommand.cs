using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.Commands.SettleDebtManually;

public record SettleDebtManuallyCommand(
    DebtId DebtId,
    UserId CreditorId) : IRequest<Result>;
