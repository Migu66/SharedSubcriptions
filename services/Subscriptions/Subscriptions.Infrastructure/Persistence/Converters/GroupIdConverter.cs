using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Infrastructure.Persistence.Converters;

internal sealed class GroupIdConverter()
    : ValueConverter<GroupId, Guid>(
        groupId => groupId.Value,
        value => new GroupId(value));
