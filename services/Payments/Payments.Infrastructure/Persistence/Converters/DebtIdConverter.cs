using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payments.Domain.ValueObjects;

namespace Payments.Infrastructure.Persistence.Converters;

internal sealed class DebtIdConverter()
    : ValueConverter<DebtId, Guid>(
        debtId => debtId.Value,
        value => new DebtId(value));
