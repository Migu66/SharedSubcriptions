using Groups.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Groups.Infrastructure.Persistence.Converters;

internal sealed class UserIdConverter()
    : ValueConverter<UserId, Guid>(
        userId => userId.Value,
        value => new UserId(value));
