using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Identity.Infrastructure.Persistence.Converters;

internal sealed class UserIdConverter()
    : ValueConverter<UserId, Guid>(
        userId => userId.Value,
        value => new UserId(value));
