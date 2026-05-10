using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payments.Domain.ValueObjects;

namespace Payments.Infrastructure.Persistence.Converters;

internal sealed class GroupIdConverter()
    : ValueConverter<GroupId, Guid>(
        groupId => groupId.Value,
        value => new GroupId(value));
