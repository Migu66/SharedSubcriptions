using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notifications.Domain.ValueObjects;

namespace Notifications.Infrastructure.Persistence.Converters;

internal sealed class NotificationIdConverter()
    : ValueConverter<NotificationId, Guid>(
        id => id.Value,
        value => new NotificationId(value));

internal sealed class GroupIdConverter()
    : ValueConverter<GroupId, Guid>(
        id => id.Value,
        value => new GroupId(value));
