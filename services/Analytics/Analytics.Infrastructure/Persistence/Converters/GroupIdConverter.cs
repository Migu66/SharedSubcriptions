using Analytics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Analytics.Infrastructure.Persistence.Converters;

internal sealed class GroupIdConverter()
    : ValueConverter<GroupId, Guid>(
        groupId => groupId.Value,
        value => new GroupId(value));
