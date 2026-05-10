using Subscriptions.Domain.Enums;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Queries.GetSubscriptionDetails;

public sealed record SubscriptionDetailsDto(
    SubscriptionId Id,
    GroupId GroupId,
    string ServiceName,
    decimal TotalCost,
    string Currency,
    BillingCycle BillingCycle,
    DateTime NextBillingDate,
    DateTime CreatedAt,
    bool IsActive);
