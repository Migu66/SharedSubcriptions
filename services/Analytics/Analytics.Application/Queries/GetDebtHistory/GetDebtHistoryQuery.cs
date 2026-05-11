using Analytics.Application.DTOs;
using Analytics.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Queries.GetDebtHistory;

public record GetDebtHistoryQuery(
    UserId UserId) : IRequest<Result<DebtHistoryDto>>;
