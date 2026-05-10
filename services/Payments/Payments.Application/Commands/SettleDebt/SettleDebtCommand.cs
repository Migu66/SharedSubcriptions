using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.Commands.SettleDebt;

public record SettleDebtCommand(
    DebtId DebtId,
    UserId DebtorId) : IRequest<Result>;
