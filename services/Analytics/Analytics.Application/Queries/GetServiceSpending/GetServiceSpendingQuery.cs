using Analytics.Application.DTOs;
using Analytics.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Queries.GetServiceSpending;

public record GetServiceSpendingQuery(
    GroupId GroupId) : IRequest<Result<IReadOnlyList<ServiceSpendingDto>>>;
