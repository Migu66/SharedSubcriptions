using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Payments.Domain.ValueObjects;

namespace Payments.Infrastructure.Persistence.Converters;

internal sealed class PaymentRecordIdConverter()
    : ValueConverter<PaymentRecordId, Guid>(
        paymentRecordId => paymentRecordId.Value,
        value => new PaymentRecordId(value));
