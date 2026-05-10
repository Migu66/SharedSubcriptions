using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payments.Domain.ValueObjects;

namespace Payments.Infrastructure.Persistence.Converters;

internal sealed class SubscriptionIdConverter()
    : ValueConverter<SubscriptionId, Guid>(
        subscriptionId => subscriptionId.Value,
        value => new SubscriptionId(value));
