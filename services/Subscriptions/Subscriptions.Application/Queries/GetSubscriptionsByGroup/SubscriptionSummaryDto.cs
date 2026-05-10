using Subscriptions.Domain.Enums;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Queries.GetSubscriptionsByGroup;

public sealed record SubscriptionSummaryDto(
    SubscriptionId Id,
    string ServiceName,
    decimal TotalCost,
    string Currency,
    BillingCycle BillingCycle,
    DateTime NextBillingDate,
    bool IsActive);
