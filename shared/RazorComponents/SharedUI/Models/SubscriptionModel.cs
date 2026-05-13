namespace SharedUI.Models;

public sealed record SubscriptionModel(
    Guid Id,
    string ServiceName,
    decimal TotalCost,
    string Currency,
    string BillingCycle,
    DateTime NextBillingDate,
    bool IsActive,
    decimal IndividualQuota);
