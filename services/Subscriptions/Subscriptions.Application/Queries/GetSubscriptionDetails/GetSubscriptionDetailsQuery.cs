using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Queries.GetSubscriptionDetails;

public record GetSubscriptionDetailsQuery(SubscriptionId SubscriptionId)
    : IRequest<Result<SubscriptionDetailsDto>>;
