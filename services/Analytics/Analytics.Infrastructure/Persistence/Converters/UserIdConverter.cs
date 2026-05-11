using Analytics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Analytics.Infrastructure.Persistence.Converters;

internal sealed class UserIdConverter()
    : ValueConverter<UserId, Guid>(
        userId => userId.Value,
        value => new UserId(value));
