using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Queries.GetSubscriptionsByGroup;

public record GetSubscriptionsByGroupQuery(GroupId GroupId)
    : IRequest<Result<IReadOnlyList<SubscriptionSummaryDto>>>;
