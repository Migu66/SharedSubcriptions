using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Enums;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Commands.CreateSubscription;

public record CreateSubscriptionCommand(
    GroupId GroupId,
    UserId AdminId,
    string ServiceName,
    decimal TotalCost,
    string Currency,
    BillingCycle BillingCycle,
    DateTime FirstBillingDate) : IRequest<Result<SubscriptionId>>;
