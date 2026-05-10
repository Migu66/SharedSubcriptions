using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payments.Domain.ValueObjects;

namespace Payments.Infrastructure.Persistence.Converters;

internal sealed class UserIdConverter()
    : ValueConverter<UserId, Guid>(
        userId => userId.Value,
        value => new UserId(value));
