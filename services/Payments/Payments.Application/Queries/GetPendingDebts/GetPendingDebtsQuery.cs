using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.Queries.GetPendingDebts;

public record GetPendingDebtsQuery(
    UserId UserId) : IRequest<Result<IReadOnlyList<DebtDto>>>;
