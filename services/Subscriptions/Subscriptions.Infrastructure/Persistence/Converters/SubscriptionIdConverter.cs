using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Infrastructure.Persistence.Converters;

internal sealed class SubscriptionIdConverter()
    : ValueConverter<SubscriptionId, Guid>(
        subscriptionId => subscriptionId.Value,
        value => new SubscriptionId(value));
